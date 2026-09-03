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
8. 重建角色 overlap 索引
```

**tile 关联策略**：当前实现取 `GetTile(x, y, z)` 返回的下方最近 tile。角色 Y 轴吸附逻辑（deltaY ≤ 0.0001f 时吸附到 tile 高度）目前被注释掉，依赖重力系统维持地面接触。

角色使用两个不同语义的索引：
- `characterTileDic`：只保存中心/支撑 owner Tile，供 `belongTile`、可见性、迷雾和编辑器放置使用。
- `characterOverlapTileDic`：保存实际 `All` Collider AABB 覆盖的 broad-phase Tile，供移动碰撞、接地、Trigger、CaptureCast 和 Object 推动使用。

`ApplyMove` 在旧坐标上完成旧位置到新位置的 Trigger 扫掠，再移动 owner、写入新的 `data.pos/euler` 并重建 overlap 索引；传送不执行移动 Trigger 扫掠。size 或 Product 变化也必须重建 overlap，但没有 owner 的非当前地图角色不得加入索引。

角色的地图边缘可行走范围随体型缩小：`CharacterProductForm.size` 映射到统一的 `CharacterUnitForm.scale` 后，非传送移动会在 `ApplyMove` 写入位置前沿连续可行走 Tile 查找真实水平外边界，并按 `max(1, scale.x) * 0.2` 钳制角色中心。size 为 `1` 时保持原来的 `0.2`；同一距离也用于角色 `BoundaryTouch`。Object 移动不使用固定内缩距离，仅当请求的目标中心真正触到或越过地图区域时触发 Object `BoundaryTouch`；通用 `InArea(Vector3)` 仍默认使用 `0.2`。

---

## 4. Interact 系统

### 4.1 CollideType 与 Mesh 分组

[MapUnit.cs:45-58](file:///d:/Works/Game/Z_Plugin/Assets/Z_Level3/Z_Map/Core/MapUnit.cs#L45-L58)

每个 MapUnit 的 Mesh 按 `CollideType` 分组缓存：
- `CollideOnly`：`isTrigger=false` 的 Collider，用于物理碰撞
- `TriggerOnly`：`isTrigger=true` 的 Collider，用于触发器事件

Mesh 在 `data.pos` 变化时重新计算（通过 `lastPos` 检测）。

### 4.2 Trigger 事件触发流程

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
3. **Mesh 缓存**：`GetMeshes` 在 `data.pos` 不变时返回缓存的 Mesh
4. **existUnit 去重**：Move 中用 `HashSet<MapUnit>` 避免同一帧重复检测同一 Unit

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

**修复**：构建导航时读取 `mapground` 的 `CollideOnly` 网格，按实际包围范围标记被占据的下层导航单元，同时排除其自身单元以保留顶面可行走。

---

### 6.8 CharacterProduct size 与大体型角色（已接入）

`CharacterProductForm.size` 为正整数，统一映射到 `CharacterUnitForm.scale`。根模型、实体 Collider 与 Trigger 会一起缩放。旧存档缺字段时使用默认值 `1`；加载、保存和 UI 输入都会把非正值归一为 `1`。

大体型角色不能使用固定九宫格 broad phase：移动与接地按实际 Collider swept AABB 枚举 Tile，其他单位通过 `characterOverlapTileDic` 查到跨格角色。寻路查询把实际水平碰撞半径换算成 footprint，并要求 footprint 内每个偏移格都能完成同一条导航边，防止中心点路径穿过过窄通道。

### 6.9 MapTexture PassType 通行约束（已接入）

Tile 使用的三层 MapTexture ID 已保存在 `TileUnitForm.texDic` 的地表槽位。每次进入场景时，将三层材质各自非零的 `MapTextureForm.passType` 聚合进 `TileUnit.passTypes`；`0` 表示该材质不增加限制。角色能力从所属 `CharacterProductForm.passType` 重算进 `CharacterUnit.passTypes`。

人物只有包含目标 Tile 的全部要求类型时才能通行。手动移动按角色实际 Collider footprint 截断在不满足类型的 Tile 前；BFS 的 clearance 检查和直线路径平滑同样检查 footprint 内每个 `NavUnit.passTypes`，因此寻路与实际移动使用一致规则。`TileUnitForm.passType` 只保留首个类型 ID 作为兼容缓存，完整要求集合不压缩进该 int 字段。

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
