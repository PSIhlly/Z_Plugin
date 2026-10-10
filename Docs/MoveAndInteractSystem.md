# Move & Interact 系统文档

## 目录

- [1. 系统总览](#1-系统总览)
- [2. 核心数据结构](#2-核心数据结构)
- [3. Move 系统](#3-move-系统)
  - [3.1 Move 入口与更新循环](#31-move-入口与更新循环)
  - [3.2 Move 核心流程](#32-move-核心流程)
  - [3.3 碰撞检测调度](#33-碰撞检测调度)
  - [3.4 SAT 碰撞检测算法](#34-sat-碰撞检测算法)
  - [3.5 避障方向计算](#35-避障方向计算)
  - [3.6 避障方向智能合并](#36-避障方向智能合并)
  - [3.7 滑行逻辑](#37-滑行逻辑)
  - [3.8 重力与地面接触](#38-重力与地面接触)
  - [3.9 位置应用与 tile 关联](#39-位置应用与-tile-关联)
- [4. Interact 系统](#4-interact-系统)
  - [4.1 CollideType 与 Mesh 分组](#41-collidetype-与-mesh-分组)
  - [4.2 Trigger 事件触发流程](#42-trigger-事件触发流程)
  - [4.3 OnEnter / OnExit / Cross](#43-onenter--onexit--cross)
  - [4.4 ObjectUnit 的 interact](#44-objectunit-的-interact)
- [5. 关键约定与约束](#5-关键约定与约束)
- [6. 已知问题与修复记录](#6-已知问题与修复记录)

---

## 1. 系统总览

Move 系统和 Interact 系统共享同一套碰撞检测基础设施，区别在于用途：

| 系统 | 目的 | 使用的 CollideType | 核心入口 |
|------|------|-------------------|----------|
| **Move** | 角色移动、碰撞截断、贴墙滑行 | `CollideOnly` | `CharacterUnit.Move()` |
| **Interact** | 单位 Trigger 事件，以及单位接触地面 Tile 的事件 | Unit 使用 `TriggerOnly`；Tile 接触使用 `CollideOnly` | `MapUpdateController.CheckCollideEvent()` |

两者底层都调用 `CheckCollide` → `MeshIntersectMesh` → `Graph.SphereIntersectCube / CubeIntersectCube / SphereIntersectSphere`。

### 调用链路

```
CharacterUnit.Move(dir)
  └─ 遍历九宫格 tile + tile 上的 Object/Character
       └─ MapUpdateController.CheckCollide(trigger, unit, dir, CollideOnly)
            └─ 遍历 trigger.GetMeshes(CollideOnly)
                 └─ CheckCollide(triggerMesh, unit, dir, CollideOnly)
                      └─ 遍历 unit.GetMeshes(CollideOnly)
                           └─ Mesh.MeshIntersectMesh(o, tar, dir)
                                ├─ Graph.SphereIntersectCube  (球-盒)
                                ├─ Graph.CubeIntersectCube    (盒-盒)
                                └─ Graph.SphereIntersectSphere(球-球)
```

---

## 2. 核心数据结构

### IntersectType — 碰撞结果类型

[Graph.cs:111-118](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level0/Z_Math/Graph.cs#L111-L118)

```csharp
public enum IntersectType
{
    None,   // 无碰撞
    In,     // 从外进入（移动前在外，移动后在内）
    Out,    // 从内脱出（移动前在内，移动后在外）
    Cross,  // 穿过（移动前在外，移动后在外，中途穿过）
    Inner   // 始终在内
}
```

### CollideType — Mesh 分组类型

[MapUtilController.cs:15-20](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level3/Z_Map/Core/MapUtilController.cs#L15-L20)

```csharp
public enum CollideType
{
    All,          // 所有 Mesh
    TriggerOnly,  // 仅触发器 Mesh（isTrigger=true 的 Collider）
    CollideOnly,  // 仅碰撞器 Mesh（isTrigger=false 的 Collider）
}
```

### MeshInfo — Mesh 几何信息

[Mesh.cs:13-19](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level1/Z_Mesh/Mesh.cs#L13-L19)

```csharp
public class MeshInfo
{
    public MeshType type;       // Cube 或 Sphere
    public Vector3[] positions; // Cube:8顶点 / Sphere:6点
    public Vector3 center;
}
```

**Cube 8 顶点顺序**（[Graph.cs:127-140](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level0/Z_Math/Graph.cs#L127-L140)）：

```
LeftDownBack,  RightDownBack,
LeftUpBack,    RightUpBack,
LeftDownForward, RightDownForward,
LeftUpForward,   RightUpForward
```

**Sphere 6 点顺序**（[Graph.cs:141-149](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level0/Z_Math/Graph.cs#L141-L149)）：

```
Up, Down, Left, Right, Forward, Back
```

### IntersectAssisant — 多 Mesh 碰撞结果聚合器

[Graph.cs:36-110](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level0/Z_Math/Graph.cs#L36-L110)

当一个 Unit 有多个 Mesh 时，`IntersectAssisant` 聚合所有 Mesh 的碰撞结果，综合考虑 `oldIn`（之前是否在内）状态，输出最终的 `IntersectType`。

---

## 3. Move 系统

### 3.1 Move 入口与更新循环

[CharacterUnit.cs:43-92](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level3/Z_Map/Core/Character/CharacterUnit.cs#L43-L92)

每帧 `UpdateInfo()` 中按顺序执行：

1. **导航移动**：若 `navEnabled` 且距离目标在 `alertDis` 内，通过 BFS 导航获取方向 `GetNavDir()`，调用 `Move(dir * speed * deltaTime)`
2. **重力**：若 `ENABLE_GRAVITY` 且 `!HasGroundContact()`，调用 `Move(Vector3.down * deltaTime * 2f)`
3. **同步实例位置**：`ins?.UpdatePos()`

### 3.2 Move 核心流程

导航端点使用每个人物独立的 `NavigationEndpointState`：起点格不可通行（Object 阻断、缺格、空格、通行类型或人物 footprint 不满足）时，先直线移动到距离起点最近的可走格中心，再恢复 BFS。目标格不可通行时，在 BFS 可达格中选择距离目标世界坐标最近的一格，到达格中心后再直线接近原目标坐标。末段状态不会因为人物进入目标阻断格而重新触发脱离；目标换格、体型/能力变化、停用导航和传送均重置状态。同格内的移动目标沿用当前阶段，但始终朝最新坐标移动；半径比较使用 `0.0001` 容差，避免平移的浮点误差重置阶段。搜索步数耗尽不能当成已到达末段起点。直线端点段仅放宽导航/地形通行类型限制，仍经过 `CharacterUnit.Move` 的实体碰撞、边界限制与 Trigger 扫掠，不是传送或穿墙。每帧移动距离同时受当前路点距离约束，避免越过路点后反复折返；普通手动移动的 passType 限制保持不变。

[CharacterUnit.cs:103-269](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level3/Z_Map/Core/Character/CharacterUnit.cs#L103-L269)

```
Move(dir):
  dirQue = Queue([dir])           // 移动方向队列，支持多次滑行重试
  firstTry = true

  while dirQue.Count > 0:
    dir = dirQue.Dequeue()
    mag = dir.magnitude
    res = mag                      // 当前可移动最短距离
    avoidDir = []                  // 本轮收集的避障法线

    ── 碰撞检测阶段 ──
    foreach tile in GetCharacterCollisionTiles(this, dir, CollideOnly):  // 当前到目标位置的 Collider swept AABB
      if tile.y < belongTile.y: continue           // 跳过下层 tile
      // 不按 tile 锚点距离筛选；实际 Collider 可能跨逻辑层

      casts = [tile] + tile.objects + tile.characters
      foreach obj in casts:
        if obj is ObjectUnit && !obj.isObstacle: continue
        if obj == this || alreadyChecked: continue

        cur = CheckCollide(this, obj, dir, CollideOnly, out avoid)

        if cur ≈ 0 && res ≈ 0:       // 已嵌入：合并避障方向
          MergeAvoidDirRange(avoidDir, avoid)
        elif cur < res:               // 更短碰撞距离：替换
          res = cur
          avoidDir.Clear()
          MergeAvoidDirRange(avoidDir, avoid)

    ── 滑行计算阶段 ──
    if (firstTry || res > 0.001) && avoidDir.Count > 0:
      if IsVectorsInHemisphere(avoidDir, out normal):  // 同半球
        newDir = Σ(avoidDir)
        if newDir 不平行于 dir:
          dir = dir * (mag - res) / mag          // 截断到碰撞点
          loss = |dir| - Dot(newDir, dir)
          slideDir = dir - (Dot(newDir,dir) - 0.1*loss) * newDir
          dirQue.Enqueue(slideDir)                // 滑行方向入队

    ── 应用移动 ──
    firstTry = false
    dir *= res / mag
    if res > 0.01:
      ApplyMove(this, pos + dir, euler)
      moved = true

  if moved:
    累计本次实际水平位移
    if !isMine && 累计距离 > abs(speed) / 3:
      朝向 = LookRotation(本次实际水平移动方向)
      清空累计距离
    触发 BoundaryTouch / Move 事件
```

**关键设计**：
- **移动队列**：初始方向被墙阻挡后，计算滑行方向重新入队，支持多次重试
- **首次尝试标记**：`firstTry=true` 时即使 `res=0` 也计算避障；后续迭代要求 `res>0.001` 才滑行，防止无限循环
- **邻域遍历**：只检测 `GetNineTile` 返回的有界邻域，且跳过低于当前层的 tile。候选不能再按 Tile 锚点的三维距离排除，跨层 Collider 由 Mesh 的 AABB/SAT 判定实际是否接触
- **通行类型**：只检查人物中心所属的逻辑 Tile，不读取 Collider/Trigger，也不因人物尺寸或相邻 Tile 受限而提前拦截。普通手动移动的目标中心落入不满足 `passType` 的 Tile 时，通过带通行类型过滤的最近有效位置搜索，把中心放到距离目标最近的合法 Tile 内；导航端点的直线段使用本节开头的定向例外，正常 BFS 路段仍要求通行类型匹配。
- **Object 覆盖**：从 `tile.data.pos` 向上探测 `0~1` 世界单位（包含两端），这条竖直线段与 Object 本体相交时，该 Tile 的运行时通行类型暂按 default（无要求）；纹理派生的原始要求保留，最后一个覆盖 Object 离开后恢复。不是单点检测、无限向上射线或仅 X/Z 投影。优先使用 `GetMeshes(CollideOnly)` 的实际 Box/Sphere 本体，考虑 `MapModel.colliderScale`、嵌套变换及 Collider.center，排除放大的 Trigger；Sphere 半径遵循现有物理碰撞换算。池模板根节点 inactive 不影响 Collider 提取，但模板中的 inactive 子节点仍排除。没有支持的物理本体时才回退原有模型/Renderer/锚点盒；组合回退模型中心使用实际 `localPosition`（含拼装 +0.5Y），不能扣掉偏移。精确探测结果单独维护在 `objectPassTypeTileDic`，仅枚举每个本体包围盒向下扩展 1 后的附近 Tile，并用线段/旋转盒 SAT 或线段/球距离确认相交；显示/owner 索引仍不受 colliderScale 影响。Object 增删、移动、旋转或显式尺寸刷新时只更新旧/新视觉、探测及物理导航覆盖 Tile 的并集：复用缓存地形高度，重算附近障碍和通行类型，再更新这些格的出向连接及四邻格（Y 偏移 -1..1）的入向连接。不触发全图重建或重新采样所有 Tile；原有周期重建完成后只补刷其期间发生 Object 变化的覆盖格。
- **Object 导航几何**：从 `GetMeshes(CollideOnly)` 读取实际 Box/Sphere 本体，考虑 collisionScale、嵌套变换和 Collider.center，排除 Trigger。先用 Tile 中心的 `0.8×0.8` 方形判定自身是否可站立（`NavUnit.objectBlocked`）；四向分别用长边贴合整条对应边、向内深 `0.4` 的矩形判定（单位格为 `1×0.4`，包含边角），保存在 `objectBlockedDirections` 的 Right/Left/Forward/Back 四位中。连接要求双方中央都可站立，且出发格对应边和目标格反向边都无阻挡；不因边上有障碍就把两个中央都判为不可走。各区域都求真实物理相交：Box 面用复用数组裁剪，Sphere 求裁切范围内最高相对高度；任一 `isObstacle` Object 在相交处高出该位置地面严格超过 `0.3` 世界单位才阻挡，等于 `0.3` 不阻断（几何容差 `0.0001`）。不是自身厚度、四点采样或 AABB；测试范围以外的全局最高点无关。只接触区域边界、水平尺寸为零、地面以下本体不阻断；薄桥高出地面 `0.25` 仍通行。地面平面复用 `dirGroundY`，不写入 Object 顶面。中央可站立但四边全断的格子仍可站立，不触发起点/终点不可走的直线补偿；实心 mapground 的阻断仍单独检查。BFS 体型 footprint 逐格检查方向连接，直线优化也要求扩展矩形内部的连接存在，不能跨过边界障碍。`objectNavigationTileDic` 继续独立保存物理候选，未启用 `isObstacle` 时也建立；小于 `0.8` 的格尺寸要让候选范围覆盖中央方形。完整/局部更新共用有界枚举，只重算旧/新覆盖格的中央及四向状态，再刷新邻格入向连接；移除一个 Object 时重查其余重叠障碍，删除最后一个后立即恢复。不重建全图，不分配每格临时几何；视觉/owner/passType 覆盖与实体移动碰撞不变。
- **斜坡连接**：继续检查四方向相向侧地面高度，条件为 `目标侧高度 - 当前侧高度 < step`，不是绝对值；因此连接按方向独立建立，可能只能下坡不能上坡。Object 中央/边界阻挡与该方向高度差条件分开判断，实心 mapground 的 `0.2` 规则也保持不变。
- **自动朝向**：非玩家角色累计实际水平位移；只有严格超过 `abs(speed) / 3` 才朝本次实际移动方向转向并清零累计值。碰撞未移动、纯 Y 位移和短距离移动均保持旧朝向

### 3.3 碰撞检测调度

#### MapUnit 级别

[MapUpdateController.cs:664-692](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level3/Z_Map/Core/MapUpdateController.cs#L664-L692)

```csharp
CheckCollide(trigger, unit, dir, type, out avoidDir)
```

- 遍历 `trigger.GetMeshes(type)` 中每个 Mesh
- 对每个 Mesh 调用 MeshInfo 级别的 `CheckCollide`
- 聚合 `IntersectAssisant`，取最短碰撞距离 `disRes`
- 避障方向合并逻辑：
  - `dis ≈ 0 && disRes ≈ 0`：已嵌入，用 `MergeAvoidDirRange` 合并
  - `dis < disRes`：更短距离，清空后合并

#### MeshInfo 级别

[MapUpdateController.cs:697-722](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level3/Z_Map/Core/MapUpdateController.cs#L697-L722)

```csharp
CheckCollide(triggerMesh, unit, dir, type, out avoidDir, out assist)
```

- 遍历 `unit.GetMeshes(type)` 中每个 Mesh
- 调用 `Mesh.MeshIntersectMesh(triggerMesh, tarMesh, dir)` 进行 SAT 检测
- 同样按最短距离原则合并避障方向（用 `MergeAvoidDir`）

### 3.4 SAT 碰撞检测算法

#### MeshIntersectMesh — 分发入口

[Mesh.cs:59-105](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level1/Z_Mesh/Mesh.cs#L59-L105)

根据 Mesh 类型组合分发：

| o \ tar | Cube | Sphere |
|---------|------|--------|
| **Cube** | `CubeIntersectCube(o, tar, step)` | `SphereIntersectCube(tar, o, -step)` |
| **Sphere** | `SphereIntersectCube(o, tar, step)` | `SphereIntersectSphere(o, tar, step)` |

> 注意：Cube-Sphere 时 step 取反，因为 `SphereIntersectCube` 的语义是"球碰盒"。

#### SphereIntersectCube — 球体与立方体 SAT

[Graph.cs:413-561](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level0/Z_Math/Graph.cs#L413-L561)

**SAT 检测轴**（共 4 个）：
1. **cube 的 3 个面法线** `normal1, normal2, normal3`（通过 `GetFaceNormals` 从 8 顶点叉积计算）
2. **球心到 cube OBB 最近点的方向** `nearestAxis`（通过 `GetClosestPointOnCube` 计算）

> 原来还有 3 个 `Cross(dir, edge)` 轴，但已被 `nearestAxis` 覆盖且在实际运行中从未生效，已删除。

**算法流程**：
```
1. 计算 sphereCenter, sphereRadius
2. 计算 cube AABB → 快速排除（distToCube > radius + mag 则 return None）
3. 计算 cube OBB 最近点 closestOnCube
4. GetPointToCube 判断球心是否在 cube 内 (fromIn)
5. 对每个 SAT 轴调用 CheckSphereAxis:
   - 投影球体和 cube 到轴上
   - CalcTouchTimeAndAvoidTime 计算 touchTime/avoidTime
6. 最终 touchTime 判定:
   - fromIn 且球远离 cube → touchTime=1 (可移动)
   - touchTime > avoidTime → touchTime=1 (无碰撞)
7. avoidDir = (contactPos - closestAtContact).normalized
   - contactPos = sphereCenter + dir * touchTime
   - closestAtContact = GetClosestPointOnCube(contactPos, cube)
```

**关键优化与修复**：
- **AABB 快速排除**：球心到 AABB 最近点距离 > `radius + mag` 时直接返回 None
- **OBB 最近点而非 AABB**：斜面等非轴对齐 cube 的 AABB 比 OBB 大，AABB 最近点会给出错误的分离轴和避障方向
- **碰撞点最近点方向作为 avoidDir**：替代 `GetPushDirByFace`，因为面分类法在棱角处选"最近面"而非"碰撞面"

#### CubeIntersectCube — 立方体间 SAT

[Graph.cs:310-381](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level0/Z_Math/Graph.cs#L310-L381)

**SAT 检测轴**（最多 6 个）：
- A cube 的 3 个面法线
- B cube 的 3 个面法线
- 通过 `AddAxisCheck` 去重（同向轴只检测一次）

#### CalcTouchTimeAndAvoidTime — 单轴时间计算

[Graph.cs:1326-1444](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level0/Z_Math/Graph.cs#L1326-L1444)

根据两个区间 `[aMin, aMax]` 和 `[bMin, bMax]` 在轴上的投影关系，以及移动方向在该轴上的投影 `dir`，计算：
- `touchTime`：球体首次接触 cube 的时间（0~1，归一化到移动距离）
- `avoidTime`：球体脱出 cube 的时间
- `avoidDir`：避障方向标记（+1/-1，表示沿轴正/负方向避让）

**区间关系分类**：
```
[] {}    aMax <= bMin        — 分离，球在负侧
[{]}     aMax > bMin && aMax < bMax && aMin < bMin  — 部分重叠，球从负侧进入
{[]}     aMax < bMax && aMin > bMin  — 球包含在 cube 内
[{}]     aMax > bMax && aMin < bMin  — 球包含 cube
{[}]     aMax > bMax && aMin > bMin && aMin < bMax  — 部分重叠，球从正侧进入
{} []    aMin >= bMax        — 分离，球在正侧
```

### 3.5 避障方向计算

#### 碰撞点最近点方向（主要方法）

[Graph.cs:537-555](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level0/Z_Math/Graph.cs#L537-L555)

```csharp
Vector3 contactPos = sphereCenter + dir * touchTime;
Vector3 closestAtContact = GetClosestPointOnCube(contactPos, cubeEightPoints);
Vector3 pushDir = contactPos - closestAtContact;
avoidDir = pushDir.normalized;
```

**为什么用碰撞点而非初始位置**：球在移动过程中，碰到的面可能和初始位置最近的面不同。用碰撞点处的最近点方向才能正确反映"球是被哪个面挡住的"。

#### GetPushDirByFace — 面分类法（回退方法）

[Graph.cs:1178-1242](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level0/Z_Math/Graph.cs#L1178-L1242)

将点转换到 cube 局部坐标系，计算到 6 个面的有符号距离：
- 内部点：选最近面（距离最大），沿该面法线推出
- 外部点：在正距离面中选距离最小的（最近接触面）

**已知问题**：棱角处选"距离最小的面"而非"碰撞面"，导致 avoidDir 方向错误。现在仅作为 `pushDir.sqrMagnitude ≤ 0.0001` 时的回退。

#### GetClosestPointOnCube — OBB 最近点

[Graph.cs:1226-1249](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level0/Z_Math/Graph.cs#L1226-L1249)

```
1. 从 cube 顶点提取 right/up/forward 三个正交轴和 center
2. 计算半边长 halfX/halfY/halfZ
3. d = point - center
4. 在 cube 局部坐标系中 clamp: lx=Clamp(Dot(d,right), -halfX, halfX) 等
5. return center + right*lx + up*ly + forward*lz
```

**关键**：这是基于 OBB 的精确最近点，对斜面等非轴对齐 cube 有效。不能用 AABB 最近点（Clamp 到 min/max），因为 AABB 比 OBB 大。

### 3.6 避障方向智能合并

#### MergeAvoidDir — 单个方向合并

[Graph.cs:728-747](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level0/Z_Math/Graph.cs#L728-L747)

```csharp
MergeAvoidDir(avoidDir, newAvoid):
  if newAvoid 与 avoidDir 中任何已有方向点积 < 0（反半球）:
    return  // 丢弃 newAvoid
  avoidDir.Add(newAvoid)
```

**核心思想**：只保留同一半球内的避障方向。如果新方向与已有方向相反（点积<0），说明障碍物在不同侧，盲目合并会导致滑行方向错误。

#### MergeAvoidDirRange — 批量合并

[Graph.cs:753-759](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level0/Z_Math/Graph.cs#L753-L759)

对 `newAvoids` 中每个方向调用 `MergeAvoidDir`。

#### IsVectorsInHemisphere — 半球兼容性检查

[Graph.cs:687-725](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level0/Z_Math/Graph.cs#L687-L725)

```csharp
IsVectorsInHemisphere(vectors, out hemisphereNormal):
  foreach candidate in vectors:
    if candidate 与所有其他向量点积 >= 0:
      hemisphereNormal = candidate
      return true
  return false
```

**用途**：在 `Move` 中判断所有避障方向是否在同一半球。如果是，说明障碍物在同一侧，可以沿墙滑行；否则（如角 corner 情况）无法滑行。

### 3.7 滑行逻辑

[CharacterUnit.cs:195-228](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level3/Z_Map/Core/Character/CharacterUnit.cs#L195-L228)

```
条件: (firstTry || res > 0.001) && avoidDir.Count > 0
1. IsVectorsInHemisphere(avoidDir, out normal)  // 必须同半球
2. newDir = Σ(avoidDir).normalized              // 合成避障法线
3. newDir 不平行于 dir                          // 否则无法滑行
4. dir = dir * (mag - res) / mag                // 截断到碰撞点后的剩余距离
5. loss = |dir| - Dot(newDir, dir)              // 垂直于墙面的损失
6. slideDir = dir - (Dot(newDir,dir) - 0.1*loss) * newDir
   // 从移动方向中减去沿避障法线的分量
   // 0.1*loss 额外缩减防止贴墙卡住
7. dirQue.Enqueue(slideDir)                     // 滑行方向入队下一轮
```

### 3.8 重力与地面接触

#### HasGroundContact — 地面接触检测

[CharacterUnit.cs:277-356](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level3/Z_Map/Core/Character/CharacterUnit.cs#L277-L356)

```
1. 获取角色球体 center 和 radius
2. contactHeightLimit = 0.4 * radius   // 接触点高度阈值
3. touchRadius = radius + 0.02         // 接触判定容差
4. 遍历九宫格内的碰撞体:
   foreach mesh in obj.GetMeshes(CollideOnly):
     nearest = GetClosestPointOnCube(sphereCenter, mesh)  // Cube
            = sphereCenter 方向上的球面点                   // Sphere
     if |nearest - sphereCenter| <= touchRadius:
       if nearest.y - sphereCenter.y < contactHeightLimit:  // 接触点在球体下半部
         return true
5. return false
```

**关键**：接触点相对球心的高度 < 0.4 倍半径时才算"地面支撑"。这确保只有球体底部接触才算地面，侧面接触不算。

#### 重力施加

[CharacterUnit.cs:66-75](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level3/Z_Map/Core/Character/CharacterUnit.cs#L66-L75)

```csharp
if (GlobalSettings.ENABLE_GRAVITY && !HasGroundContact())
{
    Move(Vector3.down * Time.deltaTime * 2f);
}
```

### 3.9 位置应用与 tile 关联

#### ApplyMove

[MapUpdateController.cs:540-600](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level3/Z_Map/Core/MapUpdateController.cs#L540-L600)

```
ApplyMove(unit, newPos, euler, teleport=false):
1. 记录 oldPos 与角色旧 overlap Tile
2. 非传送角色按 `size * 0.2` 将 newPos 钳制在地图水平外边界以内
3. newPos → mapPos；越界时用 GetClosestInArea 拉回
4. newMap = GetTile(mapPos.x, mapPos.y, mapPos.z)  // 下方最近 tile
5. if !teleport && oldPos != newPos:
     CheckCollideEvent(unit, newPos - oldPos, oldOverlap)
     // 此时 Mesh 仍位于 oldPos，检测段正好是 oldPos → newPos
6. 更新 owner DoubleDictionary (unit ↔ tile)
7. 写入 ins.transform 与 data.pos/euler
8. 查询完整角色覆盖格，增量删除离开的关联并加入新覆盖的关联
```

**tile 关联策略**：当前实现取 `GetTile(x, y, z)` 返回的下方最近 tile。角色 Y 轴吸附逻辑（deltaY ≤ 0.0001f 时吸附到 tile 高度）目前被注释掉，依赖重力系统维持地面接触。

Mod 的 Object、Item、Character 放置高度不再有上限，最低值仍为 0。放置笔刷与 ModAsset API 共用 `GetPlacementTile(worldPosition)`：沿用 `RealPos2MapPosInt` 的格尺寸换算和逻辑坐标取整，从目标层在同一 X/Z 列向下找最近 Tile；相机当前层为空时也适用。实际放置的世界高度与角度保留，不吸附到 belong 的高度。完全没有当前/下方 Tile 时拒绝放置，不生成 Form 行或历史操作，也不清空 redo。Item/Character 底层创建改为关联查到的 Tile，避免直接读取空层字典键；Character 原有运行时最近地图区域回退保留，但 Mod 会先拒绝无支撑列。

ModScene 无笔刷点击 Object（普通编辑及 Event 编辑）以 `CameraInstance.tarTrs.position.y` 为当前编辑高度：owner Tile 高于此平面，或全部有效物理 Collider 的世界下界高于此平面时先跳过该 Object，继续检查后续射线命中，优先选中符合当前高度条件的目标。只有本次没有选中任何有效目标时，才回退选中第一个被跳过的 Object，沿用射线由近到远（俯视/侧视从高到低）的顺序。普通模式忽略 Tile，Event 模式选中的 Tile 也算有效目标，不再触发高层 Object 回退。高度比较保留 `0.0001` 容差，任一本体触及/跨越平面仍可选中；读取 live `Collider.bounds`，包含子模型偏移、旋转、缩放及贴图拟合，不因 root/belongTile 较低或放大的 Trigger 延伸到本层而优先误选。忽略 disabled/inactive 本体；仅 Trigger 的 prefab 使用命中 Trigger 的下界。该规则不修改 owner 索引、放置/擦除、其它单位选择或 Play 碰撞。

角色使用两个不同语义的索引：
- `characterTileDic`：只保存中心/支撑 owner Tile，供 `belongTile`、可见性、迷雾和编辑器放置使用。
- `characterOverlapTileDic`：保存实际 `All` Collider AABB 覆盖的 broad-phase Tile，供移动碰撞、接地、Trigger、CaptureCast 和 Object 推动使用。

`ApplyMove` 在旧坐标上完成旧位置到新位置的 Trigger 扫掠，再移动 owner、写入新的 `data.pos/euler` 并增量更新 overlap 索引；传送不执行移动 Trigger 扫掠。size 或 Product 变化也必须重新查询完整覆盖范围，但没有 owner 的非当前地图角色不得加入索引。

角色的地图边缘可行走范围随体型缩小：`CharacterProductForm.size` 映射到统一的 `CharacterUnitForm.scale` 后，非传送移动会在 `ApplyMove` 写入位置前沿连续可行走 Tile 查找真实水平外边界，并按 `max(1, scale.x) * 0.2` 钳制角色中心。size 为 `1` 时保持原来的 `0.2`；同一距离也用于角色 `BoundaryTouch`。Object 移动不使用固定内缩距离，仅当请求的目标中心真正触到或越过地图区域时触发 Object `BoundaryTouch`；通用 `InArea(Vector3)` 仍默认使用 `0.2`。

---

## 4. Interact 系统

### 4.1 CollideType 与 Mesh 分组

[MapUnit.cs:45-58](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level3/Z_Map/Core/MapUnit.cs#L45-L58)

每个 MapUnit 的 Mesh 按 `CollideType` 分组缓存：
- `CollideOnly`：`isTrigger=false` 的 Collider，用于物理碰撞
- `TriggerOnly`：`isTrigger=true` 的 Collider，用于触发器事件

预制、旋转或缩放变化时重建以原点为基准的几何缓存；仅位置变化时，从缓存偏移更新已有世界 Mesh 和顶点数组，不再扫描 Collider 或创建新 Mesh。不要累加平移差值，避免浮点误差累积。`UpdateSingleOne` 会显式失效缓存以支持同名预制编辑。返回的 Mesh 是实时缓存，不是可长期保存的快照。

MapObject 的 `boundsCollision` 默认关闭；开启后按贴图 alpha ≥ 0.3 的像素边界框收缩、偏移 Collider（8 位 alpha 为 77～255），忽略低透明度杂点。贴图横向对应 X，纵向对应 Object 的 Y/Z 对角线：归一化纵向跨度为 `h` 时，高度为 `Y*h`、深度为 `Z*h`，模型及 root 缩放后的 Y:Z 比例自然保留，不再额外乘一次 Y/Z 权重。纵向中心偏移也同时分配到 Y/Z；例如高度 10、深度 1、纵向占比 0.4 时，得到高度 4、深度 0.4。首个模型部件只使用**当前朝向**的动画贴图，四向之间不合并；多帧和 GIF/WebP 仅合并该朝向的所有帧边界。运行时用 `GetAnimDirection(data.euler.y)` 选择方向，外观预览通过 `previewDirection` 指定选中的方向；明确为空的方向不回退到模型的旧贴图。其他部件分别使用自己的贴图。尺寸与现有 `colliderScale` 叠加，中心偏移按未缩小的贴图空间计算，转换后的 Sphere 还需抵消 Collider 的 Y 缩放；自动 Trigger 保留三个轴的原有边缘余量。Sphere 开启后转换为 Box，三个轴都参与收缩，保留原有组件缩放；已禁用、等待销毁的 Collider 不参与 Mesh 提取。全透明或所有像素均低于阈值的贴图 XYZ 为零，空槽或缺失贴图保留原尺寸。预览、显示实例和屏幕外 Mesh 共用 `MapUtilController.GetTextureBoundsBox`；不会改变源预制、模型或 Renderer。物理碰撞、passType 竖直探测、导航读取同一套调整后的 Mesh；视觉覆盖和 WangTile 连接继续使用原始模型边界。旧存档缺字段时仍按关闭处理。

同时开启 `isWangTile` 时，首个部件的 bounds 改用**当前方向、当前八邻接 mask 拼合后的图片**，不能扫描原始 4×6 四分块图集，也不能混入其他方向。普通四向和 WangTile 的首部件都按产品/方向/mask 缓存（普通贴图用 mask=-1）；首次遇到该变体时计算，后续刷新及动画切帧不重复扫描，注册新帧、编辑贴图、产品重新生成和场景卸载时失效。所有启用 bounds 的池预制均保留完整首部件基准 Box，MapUnit 保存归一化 bounds 并在 `GetMeshes` 中应用，因此屏幕外物体也有正确碰撞；显示实例每次从基准重新计算 Collider，避免对象池复用或多次刷新造成累积收缩。转向沿既有 Object Move/Refresh 外观通知同步更新当前方向的 Collider、Trigger、通行探测及导航索引；WangTile 邻居增删、移动、显式刷新还更新受影响的未显示邻居。场景加载要等候选列索引完整后统一解析；Mod 在既有 Collider 缩放归一为 1 后再次解析。场景缓存重置和移除同时释放普通/四向/WangTile 的 bounds 覆盖。外观预览采用选中方向，WangTile 用 TileHelper 拼合的无邻居 mask 0；其他部件仍按各自普通贴图计算。

### 4.2 Trigger 事件触发流程

Object 的 `centerCollider` 默认开启，旧存档缺字段时仍居中；关闭时，最终碰撞本体在局部 -Z 方向的后边界固定贴合组合模型后边界。先完成 `colliderScale` 和可选贴图边界拟合，再统一平移所有 Collider/Trigger；X/Y、模型 Renderer、视觉 owner/WangTile 边界保持不变，Trigger 原有余量保留。池预制仍保存居中基准，MapUnit 保存逐单位开关，屏幕外 Mesh 和显示实例使用同一偏移计算；实例 Show/显式刷新先恢复基准，避免对象池复用累积偏移。零尺寸、放大、根旋转缩放也走同一流程。ModScene 既有 Collider scale=1 规则不变，重置后重新解析后边界贴合。

贴合的是两者的后边界，不是碰撞体中心。轴向 Box 满足 `Collider中心Z = Object后边界Z + 最终Collider深度/2`；单位深度模型在 `colliderScale=0.2` 时，后边界为 `-0.5`，碰撞体中心为 `-0.4`。回归除 Mesh 等价外，还独立用实际 BoxCollider 变换后的八顶点检查该关系。

[MapUpdateController.cs:601-659](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level3/Z_Map/Core/MapUpdateController.cs#L601-L659)

```
ApplyMove(unit, newPos, euler):
  └─ if oldPos != newPos:
       CheckCollideEvent(unit, newPos - oldPos)

CheckCollideEvent(unit, dir):
  1. 合并角色移动前后的 Collider 覆盖格
  2. 对每个候选 Tile 使用 CollideOnly 检测实体接触，并交给 ManageTriggerEvent
  3. 遍历覆盖范围内的所有 Object/Item/Character（Character 查 overlap 索引）
  4. 对每个目标 tar:
       CheckCollide(unit, tar, dir, TriggerOnly, onCast)
       └─ onCast = (tar, res, dis) => ManageTriggerEvent(unit, tar, res)

ManageTriggerEvent(a, b, type):
  └─ 在 LateUpdate 中执行:
       switch type:
         In:    b.OnEnter(a); a.OnEnter(b)      // 进入触发器
         Out:   b.OnExit(a);  a.OnExit(b)       // 离开触发器
         Cross: b.OnEnter(a); a.OnEnter(b)      // 穿越（一帧内进入又离开）
                b.OnExit(a);  a.OnExit(b)
```

**关键**：
- Trigger 检测使用 `TriggerOnly` 类型的 Mesh
- 事件延迟到 `LateUpdate` 执行，避免移动过程中状态不一致
- `teleport=true` 时跳过整段移动 Trigger 扫掠（传送不产生 Enter/Exit/Cross）

### 4.3 OnEnter / OnExit / Cross

[Unit.cs:207-235](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level2/Z_UnitSystem/Core/Unit.cs#L207-L235)

```csharp
OnEnter(unit):
  if !collidingUnitUid.Contains(unit.uid):
    collidingUnitUid.Add(unit.uid)
    Invoke(CollideEvent { type = TriggerEnter, a = this, b = unit })

OnExit(unit):
  if collidingUnitUid.Contains(unit.uid):
    collidingUnitUid.Remove(unit.uid)
    Invoke(CollideEvent { type = TriggerExit, a = this, b = unit })
```

**状态管理**：`collidingUnitUid` 记录当前正在碰撞的 Unit UID，防止重复触发。`IntersectAssisant` 的 `oldIn` 参数即来自此集合。

移动单位与 Tile 的实体 Collider 首次接触时也沿用这套状态链路，但几何筛选使用 `CollideOnly`。`CollideEvent` 的移动单位一侧在 `TriggerEnter` 时执行 `onTileTouchEvent`，heap 中 `self` 是移动单位、`target` 是接触到的 Tile；离开 Tile 只清理接触状态，不额外执行事件。该事件与地图外边缘触发的 `onBoundaryTouchEvent` 相互独立，传送移动仍不扫描。

### 4.4 ObjectUnit 的 interact

[ObjectUnit.cs:61-107](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level3/Z_Map/Core/Object/ObjectUnit.cs#L61-L107)

ObjectUnit 也有 `Move` 方法，用于物体移动时推开角色：

```
ObjectUnit.Move(dir):
  targetPos = pos + dir
  touchBoundary = !InArea(targetPos, 0)  // 只检查真实地图边缘，不使用固定0.2内缩

  if isObstacle:
    foreach tile in GetOverlap(data):
      foreach ch in tile.characters:
        dis = CheckCollide(this, ch, dir, CollideOnly)
        if dis < mag:
          push[ch] = -dir * 1.1 / mag * (dis - mag)  // 反向推力

  ApplyMove(this, targetPos, euler)

  foreach (ch, pushDir) in push:
    ch.Move(pushDir)  // 推开角色

  if touchBoundary:
    Invoke(BoundaryTouch)
  else:
    Invoke(Move)
```

**特点**：ObjectUnit 移动时不做滑行，而是直接应用移动并推开挡路的角色。Object 的边界事件基于移动前计算出的请求目标中心，因此即使 `ApplyMove` 将越界位置拉回地图内，仍会触发一次 `BoundaryTouch`；仅处于边缘内侧的 `0.2` 范围不会触发。

---

## 5. 关键约定与约束

### 工程约定（来自 project_memory）

1. **有界邻域遍历**：Move 系统只检测 `GetNineTile` 返回的附近 Tile，并跳过低于当前层的 Tile
2. **跨层 Collider**：不能用 Tile 锚点距离过滤碰撞候选；例如上层 `mapground` 的 Collider 会向下覆盖下一逻辑层，必须进入实际 Mesh 检测
3. **避障方向合并**：必须用 `MergeAvoidDir/MergeAvoidDirRange`，禁止 `AddRange` 盲目并集
4. **碰撞避障方向计算**：使用碰撞点处球心到 OBB 最近点的方向，不使用 `GetPushDirByFace`（棱角处错误）
5. **OBB 最近点**：用 `GetClosestPointOnCube`，不能用 AABB 最近点（斜面等非轴对齐 cube 会出错）
6. **角色 Y 轴吸附**：仅当 `deltaY ≤ 0.0001f`（非爬升状态）时吸附到 tile 高度，保留 slideDir 的 +Y 爬升分量
7. **重力施加条件**：`HasGroundContact()` 返回 false 时才施加重力，接触点高度阈值 0.4 倍半径
8. **相机 Isometric 模式**：角度固定 45 度，不使用动态计算
9. **人物自动朝向**：非玩家人物按实际水平位移累计，累计距离严格超过 `abs(speed) / 3` 时才更新到最近一次实际移动方向；`forceEuler` 会清空累计距离

### 性能优化

1. **AABB 快速排除**：`SphereIntersectCube` 开头用 AABB 最近点距离 > `radius + mag` 直接返回 None
2. **SAT 轴去重**：`AddAxisCheckSphere` 检查轴是否已存在（含反向），避免重复检测
3. **Mesh 与 mapground 覆盖缓存**：平移只更新已有顶点；预制、旋转或缩放变化才重建几何。周期导航刷新复用每个 `mapground` 已覆盖的 NavUnit 坐标，不再重复扫描 Collider 顶点和三维候选格；仅在该 Tile 的 prefab、mapPos、pos、euler、scale、地图格尺寸变化，或导航 Tile 新增/删除/中心移动时失效。
4. **existUnit 去重**：Move 中用 `HashSet<MapUnit>` 避免同一帧重复检测同一 Unit
5. **覆盖关联增量更新**：每次仍查询完整范围（含上层），仅修改新旧覆盖格的差集；owner 索引与实际覆盖索引继续分离。
6. **遮挡最终状态提交**：先完成地块初始化、直接命中和 BFS，只记录地块最终透明度；再各遍历一次 `curObjectLst/curItemLst/curCharacterLst`，按所属地块取最终值（无 owner 或不在本帧地块集合时为 1）。Object 自动半透须先满足第 12 条 `高层 || (本层 && 非零水平碰撞体 && 最终 Collider.Y > Collider.Z)` 资格（全透与显式 `SetGroupVision` 不受此门槛影响），视觉覆盖遮挡优先，并保留列表外候选的遮挡与恢复；Play 侧视下 Object 的半透必须通过第 12 条的统一范围及按 Object 本层/高层区分的投影检查，不能由半透 owner 或混合高层覆盖绕过该条件。附属列表沿用 `UpdateSingleOne`、视野刷新和删除流程维护；公开 `SetGroupVision` 仍立即作用于一组单位。最后按实例的上次提交状态决定是否写入，复用 MaterialPropertyBlock，保留其它材质属性；绑定子单位、对象池复用、显示层和 owner 变化仍需正确刷新。ModScene 的 Tile 显示仍使用层级上限；低于当前选择层的 normal/front Renderer 对将各自记录的初始 `_LightSensitivity` 减半，当前层保持初始值，Event、退出 Mod 和对象池复用时恢复初始值且不重复折半。
7. **只读字典查询**：`DoubleDictionary.Get/GetFirst` 命中时合并为一次查找，保留缺失时建空列表的兼容行为。渲染查询使用 `TryGet/TryGetFirst`，空地块和缺失 owner 不会为查询创建空列表或污染关联索引。
8. **Play 侧视 Tile/Object 统一遮挡扫描**：先枚举人物左右及自身 7 列（`dx=-3..3`），再枚举小 Z 方向后方 `0..5` 格，最后枚举本层及高 `0..5` 格，端点包含，空 Tile 只跳过不停止。一次精确 Tile 查询同时供 Tile 与 Object 使用。仅高层 Tile 可直接半透；`高度差 >= 后方距离` 是快速命中，不是唯一必要条件。不满足时继续判断 `(x, playerY, z + 高度差)` 的投影是否落在人物周围逻辑 X/Z 欧几里得半径 3 内。例如左 1、后 3、高 2 的 Tile 投影为左 1、后 1，仍必须半透；不能用原 Tile 锚点距离排除它。本层 Tile 仍不半透，范围外投影不命中兜底分支。直接命中不再受源 Tile 锚点的 3 格半径限制。保留已有 BFS 连通扩展：扩展的普通半透仍限定逻辑 X/Z 欧几里得半径 3；配置显示范围内的头顶中心 Tile 仍先触发连通块全透，不要求四邻数量，随后半透不能覆盖全透。俯视起点及 `OVERLAY_HIDE` 容差不变。Collider 高度不透明规则最高优先级：高层 `(x,highY,z)` 对应精确 `(x,playerY+1,z+highY-playerY)`，任一缓存 `CollideOnly` 身体的世界 Y 长度超过 `0.5 + 0.0001` 就保持 Tile 身体不透明、不收集该 Tile 的高层 Object 候选；不检查 Nav/passType，不计 Trigger，也不拼接分离 Collider 的高度。根/子节点旋转缩放均计入，缺失或无实体几何不保留可见。front（Renderer `3..5`）独立检查投影 Z 减一，同样位于 `playerY+1`；仅 front 命中只保留 front，不改变身体/附属状态。同帧相同 `(x,z)` 的 Collider 查询复用结果，下帧/End 清空。Object 独立遮挡仍能覆盖 Tile 的不透明例外，具体按第 12 条。全透继承、混合高层/本层的本层优先、Mod 预览及恢复清理规则保留。`DisFadeCode` 阴影不读取 `_Show`/`_FadeCenter`，仍按贴图/遮罩 Alpha 裁切。
9. **Mod 高层统一半透明**：`GlobalSettings.MOD_HIGH_LAYER_HALF_TRANSPARENT` 默认开启。`DynamicGlobalSettings.playing == false` 时，以相机目标所在 map 层为本层，当前视野内全部更高层 Tile 及其附属 Item/Character 统一使用 `_Show=1, _FadeCenter=1`，并跳过 Play 高层遮挡 BFS。Object 按视觉覆盖高层收集，因此 owner 在视野外也能半透明，高层不再受绝对高度/Collider 尺寸限制；本层须满足第 12 条最终 Collider.Y>Z，低层不自动半透；本层和低层 Tile 不受影响，本层 Object 的既有遮挡规则继续生效。进入 Play 后恢复第 8 条的普通遮挡逻辑。
10. **删除 Tile 后保持高度索引稀疏一致**：`MapInfo.UnRegisterMap` 删除某个高度后，若该 X/Z 已无任何 Tile，会一并删除空的 `mapXZ2Y` 键。视野刷新只枚举 `mapXZ2Y` 中真实登记的高度，并跳过找不到精确 Tile 数据的陈旧项，不再把 `Min..Max` 之间的空层当成 Tile；因此 Mod 的 all erase/delete all 删除整列或中间层后，下一帧 `FreshMap` 不会读取空数据。
11. **视野集合直接增量维护**：`curTileLst/newMapLst/delMapLst` 与可见 Object/Item/Character 均为无序 `HashSet`。视野边界未变化时 `FreshMap` 直接返回；普通跨格移动只枚举进入视野的非重叠薄片，`ShowAndAddLst` 仅对 `HashSet.Add` 成功的新 Tile 调用 `Show`，离开 Tile 在一次可见集合扫描中直接记录并批量移除。首次、强制刷新或新旧视野完全不重叠时才完整扫描当前视野，不再复制整个集合后用多次 `List.Remove` 求差集。附属单位根据“任一关联 Tile 仍在显示”决定最终显示状态，不依赖进入/离开集合的枚举顺序。
12. **Object 按当前枚举 Tile 前边界判定半透**：Object 自动半透资格为 `高层 || (本层 && 非零水平碰撞体 && 最终 Collider.Y > Collider.Z)`，按根位置经 `RealPos2MapPosInt` 分类，不看关联 Tile；高层不受尺寸限制，低层不合格。本层先排除自身 X 或 Z 边长为零的退化 Box（旋转/纹理拟合后也按身体自身边检查，不按 AABB 猜测）；`colliderScale=0` 的草4 虽保留 Y 高度，但没有水平碰撞面积，不能因 Z=0 被判为高瘦遮挡物。再按每个缓存 `GetMeshes(CollideOnly)` 实际身体的世界 Y/Z 长度比较 Box（`Y > Z + 0.0001`，相等不合格），计入根/模型/Collider 旋转缩放、`colliderScale` 和逐单位纹理拟合；不合并分离身体，不计 Trigger，缺失物理身体不回退视觉尺寸。物理 Sphere 各轴直径相同，不能通过严格 Y>Z。删除绝对高度门槛和单独的 `colliderScale<=0.8` 分支。此资格用于独立遮挡、半透 owner/混合覆盖继承、俯视半透和 Mod 高层预览；不合格自动半透改为不透明，原有全透继承与显式 `SetGroupVision` 不变。复用第 8 条 `X +/-3 → Z 0..-5 → Y 0..+5` 的精确 Tile 查询和 `objectTileDic` 视觉反向索引，不再使用逐列收窄半菱形或向下查层。45° 侧视对每个实际变换后的视觉顶点计算世界 `z+y`，不能组合不同顶点的 AABB 最高 Y/最大 Z。仅本层 Object 投影到当前枚举 Tile 的高度平面，严格超过其前边界才半透：`max(z+y) > tile.pos.y + tile.pos.z + abs(mapUnitSize.z)/2 + 0.0001`，刚好相等不遮挡，本层不再比较人物位置。高层 Object 保留原来的 `max(z+y) > curCenterPos.z + curCenterPos.y + 0.0001` 人物平面投影判断及高层 Tile/全透继承，不施加新增 Tile 前边界门槛。根据 Object 根位置经 `RealPos2MapPosInt` 得到的逻辑 Y 判断本层/高层，不能因为本层 Object 关联了高层 Tile 就绕过新增条件。每 Object 的视觉投影和物理尺寸资格各缓存一次/帧，物理尺寸复用现有身体 Mesh，不重新枚举 Collider 组件；仅已命中的 Object 可跳过，首次 Tile 边界不满足时仍检查之后的关联 Tile，避免枚举顺序影响结果。先解析独立合格对象，再处理高层/owner 继承，合格独立半透优先于 owner 全透；Play 侧视的半透 owner 和混合高/本层覆盖必须通过同一范围及对应本层/高层的投影条件。范围外原有全透继承、俯视继承和 Mod 高层预览的其它规则仍保留（自动半透统一受层级/Collider 资格限制）。Item/Character 继续继承 Tile；列表外对象也能查询并恢复。移动、旋转、缩放、模型位移、关联 Tile 变化会在下帧生效。查询不创建空索引键，视觉投影/Collider 资格缓存及合格集合每帧与 End 清理，组合预制稳定帧无临时几何分配。

13. **相机中心逐像素渐变**：`DisFadeCode` 新增 `_FadeCenter`（Inspector 为 `fadeCenter`，默认 0）。CPU 的 `0.5` 仅保留为半透状态分类，不再传给 `_Show`；本帧需要半透的已显示 Renderer 提交 `_Show=1, _FadeCenter=1`，普通显示清为 `_FadeCenter=0`，全透/未显示提交 `_Show=0, _FadeCenter=0`。普通部分与 front 独立，附属/绑定单位沿用各自最终状态；离开遮挡、显示层切换和对象池复用都重置标记。Shader 逐像素从实际相机视轴中心反解地面 X/Z 投影，按欧几里得世界距离 `d` 使用 `visibility=0.2+0.8*saturate((d-1.5)/(3.5-1.5))^2`：距中心 1.5 以内保持可见度 0.2，仅 1.5～3.5 的两世界单位环带平方渐变，3.5 及以外完全不透明（距 2.5 时可见度为 0.4）。中心保留 0.2 下限；地图半径减一逻辑仍取消。固定世界半径不随格尺寸改变，缩放、宽高比和相机倾角反解仍保留；CPU 遮挡搜索/BFS 范围不改，共享 `_MapFadeCenterRange.xy=3*abs(mapUnitSize.xz)` 仍按原协议写入，但 Shader 不再读取它计算半径，无地图全局值的预览采用相同 1.5～3.5 范围。透视模式以 `_MapFadeCenterPosition=curCenterPos` 的深度作为范围参照。Tile/Object 候选复用第 8 条统一扫描，普通 Tile 连通扩展保留 X/Z 半径，Object 使用第 12 条按本层/高层区分的投影判定，碰撞/导航不变；显式非半透值如 `0.25` 仍走原 `_Show`，阴影不读取 `_Show` 或 `_FadeCenter`。 为消除半透杂点，Forward 不再使用屏幕空间随机抖动裁剪，改为预乘 Alpha 的连续混合（`Blend One OneMinusSrcAlpha`、`ZWrite Off`、`ZTest LEqual`），输出 `diffuse*finalAlpha` 与 `finalAlpha`；只有不可见像素被裁掉，贴图/遮罩本身的 Alpha Cutoff 保留。多层重叠按普通透明排序叠加（两个 0.25 层为 0.4375），不再采用旧点阵最大覆盖规则；ShadowCaster 的深度写入与轮廓不变。

14. **本层 owner 的高处 Object 连通全隐藏**：Play 从人物中心的精确本层 Tile 出发，沿本层四邻格在已有 view 矩形内 BFS；仅当该 Tile 真正拥有一个高处 Object 才继续扩展。使用 `objectTileDic.TryGetFirst(object) == tile` 排除其它 owner 的视觉覆盖。高处定义为全部有效物理 Collider 的世界下界严格大于所属本层的上界：`minY > tile.data.pos.y + 1 + 0.0001`；即相对 Tile 高度超过 1 世界单位，不是只离开地面，也不是固定世界坐标 Y>1。计入子模型偏移、旋转缩放与纹理拟合，不依赖 root 的逻辑层。复用 `GetMeshes(CollideOnly)`，仅无物理本体时回退 `TriggerOnly`；任一本体仍在本层内/穿过本层、下界等于上界或缺失几何均不符合，Sphere 使用物理半径求真实下界。只把该连通块中符合条件的 Objects 设为 degree=0，本层 Tile/front、层内 Object、Item 和 Character 不受此路径影响。该状态在独立/混合半透之后、owner 继承之前写入，不能被半透重新覆盖；列表外对象也能隐藏和恢复。空格、缺格、只有层内对象的格子阻断，斜对角和非 owner 覆盖不连通；全隐藏不受半透半径限制。人物离开、对象降回本层/迁移 owner、连接断开后下一帧沿既有最终状态流程恢复。复用已有 BFS 队列和 visited，查询不创建索引键，不扫全图，稳定帧不分配临时几何；Mod 高层预览不运行此 Play 路径，普通半透、ModScene 点击筛选及原有 Tile Collider 不透明例外不变。

隔离回归入口：`Docs/Tests/Run-MapRuntimeRegression.ps1`。它编译真实生产程序集，并在 Temp 下的独立 Unity 工程验证几何等价、双向覆盖关联、通行类型移动边界、遮挡提交和事件过滤；`-Fixture MapFadeShaderRegression` 使用实际 Shader 的 GPU 像素验证渐变和阴影，不加载当前故事或玩家存档。实际移动、完整 Play 画面和目标设备性能仍需 Play/Player 复测。

---

## 6. 已知问题与修复记录

### 6.1 SphereIntersectCube touchTime=0 误判（已修复）

**现象**：球体未接触 cube，但 SAT 报告 touchTime=0（已重叠），导致角色卡住。

**根因**：球心在 cube 棱/角附近时，3 个面法线轴的投影都重叠，但实际球体未接触 cube。缺少棱角处的分离轴。

**修复**：添加 `nearestAxis`（球心到 OBB 最近点方向）作为第 4 个 SAT 轴。

### 6.2 avoidDir 方向错误导致无法滑行（已修复）

**现象**：球沿 -X 方向撞到 cube 的 +X 面，但 avoidDir 返回 +Z 方向（贴住的面而非撞上的面），滑行逻辑去掉 +Z 分量后角色滑进墙里。

**根因**：`GetPushDirByFace` 在棱角处选"距离最小的面"（最近面），而非"球撞上的面"。

**修复**：用碰撞点处球心到 OBB 最近点的方向作为 avoidDir，替代 `GetPushDirByFace`。

### 6.3 斜面碰撞 touchTime 虚增（已修复）

**现象**：角色上斜面时 touchTime 从正确的 0.079 被虚增到 0.502，avoidDir 方向错误。

**根因**：nearestAxis 和 avoidDir 使用 AABB 最近点，但斜面 cube 的 AABB 比 OBB 大，AABB 最近点方向不是真正的分离轴。

**修复**：nearestAxis 和 avoidDir 都改用 `GetClosestPointOnCube`（OBB 最近点），AABB 仅用于快速排除。

### 6.4 cross(dir, edge) 轴冗余（已移除）

**现象**：`SphereIntersectCube` 中 3 个 `Cross(dir, edge)` 轴在实际运行中从未生效（被 `AddAxisCheckSphere` 去重跳过）。

**修复**：删除这 3 个轴的检测，nearestAxis 已覆盖棱角分离轴的功能。

### 6.5 避障方向盲目并集导致滑行错误（已修复）

**现象**：多个碰撞面的 avoid 方向盲目取并集后，冲突方向污染滑行计算。

**修复**：用 `MergeAvoidDir/MergeAvoidDirRange` 替代 `AddRange`，只合并同半球兼容的方向。最近碰撞面的 avoid 优先加入，冲突方向的远端 avoid 被丢弃。

### 6.6 跨层 Tile Collider 被锚点距离过滤（已修复）

**现象**：`mapground` 的 Collider 向下覆盖下一层，但人物移动和接地检测仍可穿过。

**根因**：候选 Tile 在进入 Mesh 检测前使用 `sqrMagnitude > 1.69` 过滤；相邻逻辑层的锚点高度差为 1.5，平方已是 2.25，实际重叠的 Collider 被提前排除。

**修复**：保留有界邻域和下层跳过规则，移除 Tile 锚点距离过滤，由 Mesh AABB/SAT 决定是否接触。

### 6.7 mapground 下层未进入寻路阻挡集（已修复）

**现象**：导航仍会规划经过 `mapground` 实体占据的下一层。

**根因**：`NavigationController` 的 `blocked` 集合没有根据 Tile Collider 填充。

**修复**：构建导航时读取 `mapground` 的 `CollideOnly` 网格，按实际包围范围标记被占据的下层导航单元，同时排除其自身单元以保留顶面可行走。覆盖结果按 `mapground` Tile 缓存；周期刷新直接合并缓存坐标，只有 Tile 几何签名或导航格布局变化时才重新计算。

---

### 6.8 CharacterProduct size 与大体型角色（已接入）

`CharacterProductForm.size` 为正整数，统一映射到 `CharacterUnitForm.scale`。根模型、实体 Collider 与 Trigger 会一起缩放。旧存档缺字段时使用默认值 `1`；加载、保存和 UI 输入都会把非正值归一为 `1`。

大体型角色不能使用固定九宫格 broad phase：移动与接地按实际 Collider swept AABB 枚举 Tile，其他单位通过 `characterOverlapTileDic` 查到跨格角色。寻路查询把实际水平碰撞半径换算成 footprint，并要求 footprint 内每个偏移格都能完成同一条导航边，防止中心点路径穿过过窄通道。

### 6.9 MapTexture PassType 通行约束（已接入）

Tile 使用的三层 MapTexture ID 已保存在 `TileUnitForm.texDic` 的地表槽位。每次进入场景时，将三层材质各自非零的 `MapTextureForm.passType` 聚合进 `TileUnit.passTypes`；`0` 表示该材质不增加限制。角色能力从所属 `CharacterProductForm.passType` 重算进 `CharacterUnit.passTypes`。

普通移动和正常 BFS 路段要求人物包含其中心目标 Tile 的全部通行类型。手动移动的目标中心落入不满足类型的 Tile 后，会搜索并落到距离目标最近的合法位置；人物尺寸和 Collider/Trigger 不参与该判定。BFS 仍按 footprint 检查物理通道宽度，但 `passType` 只检查人物中心经过的 `NavUnit`，因此不会被相邻受限 Tile 误拦。导航起点脱离和目标末段直线接近是明确的局部例外，仍保留实体碰撞与地图边界。`TileUnitForm.passType` 只保留首个类型 ID 作为兼容缓存，完整要求集合不压缩进该 int 字段。

### 6.10 WangTile 动画帧

开启 `isWangTile` 或 `frontIsWangTile` 后，每一张配置的源动画帧都要按同一个 8 邻接 mask 拆成 WangTile 变体。相同 mask 的生成贴图按源列表顺序组成动画，并继续使用 `MapTextureForm.animTimeInterval` 切换；普通层和前景层分别维护序列。`WangTileDic` 与 `FrontWangTileDic` 保留第一张有效变体用于兼容和静态采样，完整动画缓存随场景生成、卸载和重载一起清理。

Tile（含 WangTile 的普通层与前景层）、Object、Item 的贴图动画不以实例出现时间起拍。Play 中统一用 `ProgressForm.seconds` 计算当前帧，新进入视野或从对象池恢复的实例会直接加入已有动画相位，游戏时长暂停时画面也保持当前帧。底层 `Z_Time` 通过 `animationTimeGetter` 获取该时钟，避免反向依赖存档程序集；UGC 编辑预览继续使用 `Time.time`。人物动画不使用这套全局相位。

### 6.11 桥模型偏移被导航忽略（已修复）

**现象**：Tile 的 Object 覆盖与 passType 已正确刷新，但桥仍可能没有可用导航入口。

**根因**：Play 会从 MapObject 产品的 `collision` 启用桥的障碍标志。导航原先忽略模型子节点位置，将所有 BoxCollider 都放到根位置向上 0.5，并把放大的 Trigger 计入高度。存档中本应高 0.25 的桥面因此被算成约 0.55，超过默认地图的 0.5 连通高度差。只设置 `isObstacle=false` 的桥覆盖测试无法复现此问题。

**修复**：完整与局部导航共用实际 `CollideOnly` Cube Mesh；独立索引实际物理覆盖，按地图格坐标枚举附近单元，避免世界整数范围的重复转换。回归覆盖两座桥完整岸到岸双向可达性、真实桥面高度、偏移到显示范围外/上层的碰撞体增删移动，以及局部结果与全量结果一致；不修改存档或降低障碍阈值。

**当前中央/方向规则**：使用 3.2 节的中央 `0.8×0.8` 与四向 `1×0.4` 区域，分别按 `0.3` 相对地面阈值判定，支持 Box/Sphere。桥面高出地面 `0.25` 时继续可走，不把 Object 顶面写入方向高度。中央阻挡才使用端点不可走补偿；仅边界阻挡则切断对应连接，由 BFS 绕路，不能通过补偿或直线优化穿越。实体碰撞仍然生效。

## 附录：关键文件索引

| 文件 | 职责 |
|------|------|
| [CharacterUnit.cs](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level3/Z_Map/Core/Character/CharacterUnit.cs) | 角色移动、滑行、重力、地面接触 |
| [ObjectUnit.cs](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level3/Z_Map/Core/Object/ObjectUnit.cs) | 物体移动、推开角色 |
| [MapUnit.cs](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level3/Z_Map/Core/MapUnit.cs) | Mesh 缓存、tile 关联、Create 事件 |
| [MapUpdateController.cs](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level3/Z_Map/Core/MapUpdateController.cs) | 碰撞检测调度、ApplyMove、Trigger 事件 |
| [Mesh.cs](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level1/Z_Mesh/Mesh.cs) | Mesh 构建与 SAT 分发入口 |
| [Graph.cs](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level0/Z_Math/Graph.cs) | SAT 碰撞检测算法、避障方向计算、OBB 最近点 |
| [Unit.cs](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level2/Z_UnitSystem/Core/Unit.cs) | OnEnter/OnExit 触发器基类 |
| [MapUtilController.cs](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level3/Z_Map/Core/MapUtilController.cs) | CollideType 枚举、tile 工具方法 |
