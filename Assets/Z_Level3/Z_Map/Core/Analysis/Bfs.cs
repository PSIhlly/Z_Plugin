using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Z_DesignStyle;

namespace Z_Map.Analysis
{
    /// <summary>
    /// BFS广度优先搜索寻路：基于导航网格的宏观路径规划
    /// 流程：
    /// 1. 不可走的起点先直线脱离到最近的可走节点
    /// 2. BFS遍历links；不可走的终点经最近可达节点后直线接近
    /// 3. 回溯路径，视线检测(Check)验证路径上无障碍
    /// 4. 返回可行的移动方向（去除Y分量）
    /// </summary>
    public class Bfs: NaviComponent
    {
        NavigationController nc;
        Dictionary<NavUnit, int> steps = new Dictionary<NavUnit, int>();
        Queue<NavUnit> queue = new Queue<NavUnit>();
        Dictionary<NavUnit, NavUnit> pre = new Dictionary<NavUnit, NavUnit>();
        List<NavUnit> path = new List<NavUnit>();
        readonly NavigationEndpointState scratchEndpointState = new NavigationEndpointState();
        public Bfs(NavigationController nc)
        {
            this.nc = nc;
        }
        /// <summary>
        /// BFS寻路核心方法：从cur到tar寻找可行路径，返回第一步的移动方向
        /// maxStep: 最大搜索步数限制
        /// </summary>
        public Vector3 GetNextDir(Vector3 cur, Vector3 tar, int maxStep, float agentRadius,
            IReadOnlyCollection<int> passTypes, NavigationEndpointState endpointState = null)
        {
            // 无状态的方向查询保持独立；实际角色必须传入自己的端点状态。
            if (endpointState == null)
            {
                scratchEndpointState.Reset();
                endpointState = scratchEndpointState;
            }
            endpointState.Prepare(nc, tar, agentRadius, passTypes);
            endpointState.moveTarget = cur;
            pre.Clear();
            steps.Clear();
            queue.Clear();
            path.Clear();
            var clearanceOffsets = nc.GetClearanceOffsets(agentRadius);
            if (nc.navUnits == null || nc.navUnits.Count == 0)
                return Vector3.zero;
            nc.TryGetGroundUnit(cur, out var first);
            nc.TryGetGroundUnit(tar, out var requestedEnd);
            bool targetWalkable = IsWalkable(requestedEnd, clearanceOffsets, passTypes);

            // 到达目标最近格后，即使已经进入阻断格也继续末段，不能反向脱困。
            if (endpointState.finalApproach && !targetWalkable)
                return GetDirection(cur, tar, endpointState, true);
            endpointState.finalApproach = false;

            bool startWalkable = IsWalkable(first, clearanceOffsets, passTypes);
            var escape = endpointState.escapeAnchor;
            if (escape != null && (!nc.navUnits.TryGetValue((escape.pos.x, escape.pos.y, escape.pos.z),
                    out var cachedAnchor) || cachedAnchor != escape
                || !IsWalkable(escape, clearanceOffsets, passTypes)))
            {
                escape = null;
                endpointState.escapeAnchor = null;
            }
            if (escape == null && !startWalkable)
            {
                escape = FindClosestWalkable(cur, clearanceOffsets, passTypes);
                endpointState.escapeAnchor = escape;
            }
            if (escape != null)
            {
                if (!IsAtAnchor(cur, escape))
                    return GetDirection(cur, escape.realPos, endpointState, true);
                endpointState.escapeAnchor = null;
                first = escape;
                startWalkable = true;
            }
            if (!startWalkable)
                return Vector3.zero;

            if (targetWalkable && first == requestedEnd)
                return GetDirection(cur, tar, endpointState);
            int smoothingRadiusX = 1;
            int smoothingRadiusZ = 1;
            foreach (var offset in clearanceOffsets)
            {
                smoothingRadiusX = Mathf.Max(smoothingRadiusX, Mathf.Abs(offset.x));
                smoothingRadiusZ = Mathf.Max(smoothingRadiusZ, Mathf.Abs(offset.y));
            }
            var end = targetWalkable ? requestedEnd : null;
            float minDis2 = (first.realPos - tar).sqrMagnitude;
            NavUnit minUnit = first;

            queue.Enqueue(first);
            steps[first] = 0;
            bool searchLimited = false;
            while (queue.Count > 0)
            {
                var now = queue.Dequeue();
                int step = steps[now];
                if (IsCloser(now, minUnit, tar)
                    && IsWalkable(now, clearanceOffsets, passTypes))
                {
                    minDis2 = (now.realPos - tar).sqrMagnitude;
                    minUnit = now;
                }
                if (step >= maxStep)
                {
                    foreach (var nxt in now.links)
                        if (CanPass(now, nxt, clearanceOffsets, passTypes))
                            searchLimited = true;
                    continue;
                }
                foreach (var nxt in now.links)
                {
                    if (CanPass(now, nxt, clearanceOffsets, passTypes))
                    {
                        steps[nxt] = step + 1;
                        queue.Enqueue(nxt);
                        pre[nxt] = now;

                        if (nxt == end)
                        {
                            queue.Clear();
                            break;
                        }
                    }
                }
            }
            bool reachedTarget = end != null && pre.ContainsKey(end);
            if (!reachedTarget)
            {
                //太远，说明没希望
                if (targetWalkable && minDis2 > 2*2)
                    return Vector3.zero;
                end = minUnit;
                if (end == first)
                {
                    if (targetWalkable || searchLimited)
                        return Vector3.zero;
                    if (!IsAtAnchor(cur, end))
                        return GetDirection(cur, end.realPos, endpointState);
                    endpointState.finalApproach = true;
                    return GetDirection(cur, tar, endpointState, true);
                }
            }

            Vector3 finalTarget = reachedTarget ? tar : end.realPos;

            {
                NavUnit now = end;
                path.Add(now);

                while (now != first)
                {
                    now = pre[now];
                    path.Add(now);
                }

                if (GlobalSettings.NAV_DEBUG)
                {
                    DebugPath(path);
                }
                int i = 0;

                var nxt = path[path.Count - 1].pos;
                float y = path[path.Count - 1].realPos.y;
                int forward = nxt.z;
                int back = nxt.z;
                int right = nxt.x;
                int left = nxt.x;

                //不算自己,不算终点
                for (i = path.Count - 2; i >= 0; i--)
                {
                    Vector3Int tryPos = path[i].pos;

                    int checkLeft = left;
                    int checkRight = right;
                    int checkForward = forward;
                    int checkBack = back;

                    if (tryPos.x > right)
                    {
                        right = tryPos.x;
                        checkLeft = checkRight = right;
                    }
                    if (tryPos.x < left)
                    {
                        left = tryPos.x;
                        checkLeft = checkRight = left;
                    }
                    if (tryPos.z > forward)
                    {
                        forward = tryPos.z;
                        checkForward = checkBack = forward;
                    }
                    if (tryPos.z < back)
                    {
                        back = tryPos.z;
                        checkForward = checkBack = back;
                    }
                    
                    //换层 先断
                    if (!Check(checkLeft - smoothingRadiusX, checkRight + smoothingRadiusX, nxt.y, y,
                            checkBack - smoothingRadiusZ, checkForward + smoothingRadiusZ) ||
                        !HasPassTypesOnCenterLine(nxt, tryPos, passTypes))
                    {
                        //那就只走第一步
                        if (i == path.Count - 2)
                        {
                            return GetDirection(cur, path[i].realPos, endpointState);
                        }
                        return GetDirection(cur, path[i + 1].realPos, endpointState);
                    }
                }

                if (i < 0)
                {
                    return GetDirection(cur, finalTarget, endpointState);
                }
            }
            return GetDirection(cur, finalTarget, endpointState);
        }

        private Vector3 GetDirection(Vector3 cur, Vector3 target, NavigationEndpointState state,
            bool directEndpoint = false)
        {
            state.moveTarget = target;
            state.directEndpoint = directEndpoint;
            return nc.GetNormalWithoutY(target - cur);
        }

        private static bool IsAtAnchor(Vector3 cur, NavUnit unit)
        {
            var delta = cur - unit.realPos;
            delta.y = 0f;
            return delta.sqrMagnitude <= 0.05f * 0.05f;
        }

        internal bool IsWalkable(NavUnit unit, IReadOnlyList<Vector2Int> offsets,
            IReadOnlyCollection<int> passTypes)
        {
            if (!nc.IsBaseWalkable(unit) || !HasAllPassTypes(unit, passTypes))
                return false;
            foreach (var offset in offsets)
                if (!nc.TryGetOffsetUnit(unit, offset, out var footprint) || !nc.IsBaseWalkable(footprint))
                    return false;
            return true;
        }

        private NavUnit FindClosestWalkable(Vector3 position, IReadOnlyList<Vector2Int> offsets,
            IReadOnlyCollection<int> passTypes)
        {
            NavUnit closest = null;
            foreach (var unit in nc.navUnits.Values)
                if (IsCloser(unit, closest, position) && IsWalkable(unit, offsets, passTypes))
                    closest = unit;
            return closest;
        }

        private static bool IsCloser(NavUnit candidate, NavUnit current, Vector3 position)
        {
            if (current == null)
                return true;
            float candidateDistance = (candidate.realPos - position).sqrMagnitude;
            float currentDistance = (current.realPos - position).sqrMagnitude;
            if (candidateDistance != currentDistance)
                return candidateDistance < currentDistance;
            // 等距时不依赖字典/links 枚举顺序，避免每帧选择不同的最近格。
            if (candidate.pos.y != current.pos.y) return candidate.pos.y < current.pos.y;
            if (candidate.pos.x != current.pos.x) return candidate.pos.x < current.pos.x;
            return candidate.pos.z < current.pos.z;
        }

        /// <summary>
        /// 判断NavUnit是否可通行：未被访问过
        /// </summary>
        public bool CanPass(NavUnit tar)
        {
            return nc.IsBaseWalkable(tar) && !steps.ContainsKey(tar);
        }
        /// <summary>
        /// 判断从from到tar是否可通行
        /// </summary>
        public bool CanPass(NavUnit from, NavUnit tar, IReadOnlyList<Vector2Int> clearanceOffsets,
            IReadOnlyCollection<int> passTypes)
        {
            if (!nc.IsBaseWalkable(from) || !nc.IsBaseWalkable(tar)
                || !from.links.Contains(tar) || steps.ContainsKey(tar) || !HasAllPassTypes(tar, passTypes))
                return false;

            foreach (var offset in clearanceOffsets)
            {
                if (!nc.TryGetOffsetUnit(from, offset, out var fromUnit) ||
                    !nc.TryGetOffsetUnit(tar, offset, out var toUnit) ||
                    !nc.IsBaseWalkable(fromUnit) || !nc.IsBaseWalkable(toUnit) ||
                    !fromUnit.links.Contains(toUnit))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 视线检测：验证指定矩形区域内所有导航格均无障碍且高度差在阈值内
        /// 用于判断路径上是否可以直线到达（无需绕行）
        /// </summary>
        public bool Check(int startX, int endX, int mapY, float realY, int startZ, int endZ)
        {
            for (int i = startX; i <= endX; i++)
                for (int k = startZ; k <= endZ; k++)
                {
                    if (!nc.navUnits.ContainsKey((i, mapY, k)))
                        return false;
                    var unit = nc.navUnits[(i, mapY, k)];
                    if (!nc.IsBaseWalkable(unit) || Mathf.Abs(unit.realPos.y - realY) > nc.step)
                    {
                        return false;
                    }
                    // This conservative smoothing rectangle must be internally
                    // connected. Standable centers alone cannot shortcut a wall
                    // on a shared edge; include the agent's expanded footprint.
                    if (i < endX && (!nc.navUnits.TryGetValue((i + 1, mapY, k), out var right)
                        || !nc.IsBaseWalkable(right) || !unit.links.Contains(right) || !right.links.Contains(unit)))
                        return false;
                    if (k < endZ && (!nc.navUnits.TryGetValue((i, mapY, k + 1), out var forward)
                        || !nc.IsBaseWalkable(forward) || !unit.links.Contains(forward) || !forward.links.Contains(unit)))
                        return false;
                }

            return true;

        }

        private bool HasPassTypesOnCenterLine(Vector3Int from, Vector3Int to,
            IReadOnlyCollection<int> passTypes)
        {
            int x = from.x;
            int z = from.z;
            int deltaX = Mathf.Abs(to.x - from.x);
            int deltaZ = Mathf.Abs(to.z - from.z);
            int stepX = from.x < to.x ? 1 : -1;
            int stepZ = from.z < to.z ? 1 : -1;
            int error = deltaX - deltaZ;

            while (true)
            {
                if (!nc.navUnits.TryGetValue((x, from.y, z), out NavUnit unit) ||
                    !HasAllPassTypes(unit, passTypes))
                {
                    return false;
                }
                if (x == to.x && z == to.z)
                    return true;

                int doubleError = error * 2;
                if (doubleError > -deltaZ)
                {
                    error -= deltaZ;
                    x += stepX;
                }
                if (doubleError < deltaX)
                {
                    error += deltaX;
                    z += stepZ;
                }
            }
        }

        private static bool HasAllPassTypes(NavUnit unit, IReadOnlyCollection<int> passTypes)
        {
            if (unit.passTypes == null || unit.passTypes.Count == 0)
                return true;
            if (passTypes == null || passTypes.Count == 0)
                return false;

            foreach (int requiredType in unit.passTypes)
            {
                if (!passTypes.Contains(requiredType))
                    return false;
            }
            return true;
        }
        public void DebugPath(List<NavUnit> lst)
        {
            var list = new Vector3[lst.Count];

            for (int i = 1; i < lst.Count; i++)
            {
                list[i] = lst[i].realPos + Vector3.up;
                Debug.DrawLine(list[i - 1], list[i]);
            }

        }
    }
}
