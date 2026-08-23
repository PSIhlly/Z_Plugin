using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;

namespace Z_Map.Analysis
{
    /// <summary>
    /// BFS广度优先搜索寻路：基于导航网格的宏观路径规划
    /// 流程：
    /// 1. 将起点入队，BFS遍历links中可通行的NavUnit
    /// 2. 若终点不可达，取距离终点最近的可达节点
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
        public Bfs(NavigationController nc)
        {
            this.nc = nc;
        }
        /// <summary>
        /// BFS寻路核心方法：从cur到tar寻找可行路径，返回第一步的移动方向
        /// maxStep: 最大搜索步数限制
        /// </summary>
        public Vector3 GetNextDir(Vector3 cur, Vector3 tar, int maxStep, float agentRadius)
        {

            pre.Clear();
            steps.Clear();
            queue.Clear();
            path.Clear();
            var clearanceOffsets = nc.GetClearanceOffsets(agentRadius);
            int smoothingRadiusX = 1;
            int smoothingRadiusZ = 1;
            foreach (var offset in clearanceOffsets)
            {
                smoothingRadiusX = Mathf.Max(smoothingRadiusX, Mathf.Abs(offset.x));
                smoothingRadiusZ = Mathf.Max(smoothingRadiusZ, Mathf.Abs(offset.y));
            }
            Vector3Int curPos = nc.RealPos2MapPosInt(cur);
                curPos = nc.GetClosestExistInArea(curPos);
            Vector3Int tarPos = nc.RealPos2MapPosInt(tar);
                tarPos = nc.GetClosestExistInArea(tarPos);
            if (!nc.navUnits.ContainsKey((tarPos.x, tarPos.y, tarPos.z)))
            {
                return Vector3.zero;
            }


            if (curPos == tarPos)
            {
                return nc.GetNormalWithoutY(tar - cur);
            }
            var first = nc.navUnits[(curPos.x, curPos.y, curPos.z)];
            //落地
            while (first.isNull)
            {
                first = nc.navUnits[(first.pos.x, first.pos.y - 1, first.pos.z)];
            }

            var end = nc.navUnits[(tarPos.x, tarPos.y, tarPos.z)];

            float minDis2 = (cur - tar).sqrMagnitude;
            NavUnit minUnit = first;

            queue.Enqueue(first);
            steps[first] = 0;
            int times = 0;
            //Vector3Int[] dirs = new[] { Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down, Vector3Int.forward, Vector3Int.back };
            while (queue.Count > 0)
            {
                if(times>999)
                {
                    Debug.LogError("cnm");
                    break;
                }
                var now = queue.Dequeue();
                int step = steps[now];
                if (step >= maxStep)
                    break;
                if (minDis2 > (now.realPos - tar).sqrMagnitude)
                {
                    minDis2 = (now.realPos - tar).sqrMagnitude;
                    minUnit = now;
                }
                foreach (var nxt in now.links)
                {
                    if (CanPass(now, nxt, clearanceOffsets))
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
            if (!pre.ContainsKey(end))
            {
                //太远，说明没希望
                if (minDis2 > 2*2)
                    return Vector3.zero;
                end = minUnit;
            }

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
                    if (!Check(checkLeft - smoothingRadiusX, checkRight + smoothingRadiusX, nxt.y, y, checkBack - smoothingRadiusZ, checkForward + smoothingRadiusZ))
                    {
                        //那就只走第一步
                        if (i == path.Count - 2)
                        {
                            return nc.GetNormalWithoutY(path[i].realPos - cur);
                        }
                        return nc.GetNormalWithoutY(path[i + 1].realPos - cur);
                    }
                }

                if (i < 0)
                {
                    return nc.GetNormalWithoutY(tar - cur);
                }
            }
            return nc.GetNormalWithoutY(tar - cur);
        }

        /// <summary>
        /// 判断NavUnit是否可通行：未被访问过
        /// </summary>
        public bool CanPass(NavUnit tar)
        {
            return !steps.ContainsKey(tar);
        }
        /// <summary>
        /// 判断从from到tar是否可通行
        /// </summary>
        public bool CanPass(NavUnit from, NavUnit tar, IReadOnlyList<Vector2Int> clearanceOffsets)
        {
            if (steps.ContainsKey(tar))
                return false;

            foreach (var offset in clearanceOffsets)
            {
                if (!nc.TryGetOffsetUnit(from, offset, out var fromUnit) ||
                    !nc.TryGetOffsetUnit(tar, offset, out var toUnit) ||
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
                    if (unit.links.Count == 0 || Mathf.Abs(unit.realPos.y - realY) > nc.step)
                    {
                        return false;
                    }
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