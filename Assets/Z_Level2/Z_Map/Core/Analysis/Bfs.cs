using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;

namespace Z_Map.Analysis
{
    public class Bfs
    {
        NavigationController nc;
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> pre = new Dictionary<Vector3Int, Vector3Int>();
        List<Vector3Int> path = new List<Vector3Int>();
        public Bfs(NavigationController nc)
        {
            this.nc = nc;
        }
        public Vector3 GetNextDir(Vector3 cur, Vector3 tar)
        {
            Vector3Int curPos = nc.RealPos2MapPos(cur);
            if (!nc.InArea(curPos))
                curPos = nc.GetClosestInArea(curPos);
            Vector3Int tarPos = nc.RealPos2MapPos(tar);
            if (!nc.InArea(tarPos))
                tarPos = nc.GetClosestInArea(tarPos);

            if (curPos == tarPos)
            {
                return (tar - cur).normalized;
            }

            pre.Clear();
            visited.Clear();
            queue.Clear();
            path.Clear();

            queue.Enqueue(curPos);
            visited.Add(curPos);

            Vector3Int[] dirs = new[] { Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down, Vector3Int.forward, Vector3Int.back };
            while (queue.Count > 0)
            {
                var now = queue.Dequeue();

                foreach (var dir in dirs)
                {
                    var nxt = dir + now;
                    if (CanPass(nxt))
                    {
                        visited.Add(nxt);
                        queue.Enqueue(nxt);
                        pre[nxt] = now;
                        if (nxt == tarPos)
                        {
                            queue.Clear();
                            break;
                        }
                    }
                }
            }

            if (pre.ContainsKey(tarPos))
            {
                Vector3Int now = tarPos;
                path.Add(now);

                while (now != curPos)
                {
                    now = pre[now];
                    path.Add(now);
                }
                //DebugPath(path);
                int i = 0;

                int y = path[path.Count - 1].y;
                int forward = path[path.Count - 1].z;
                int back = path[path.Count - 1].z;
                int right = path[path.Count - 1].x;
                int left = path[path.Count - 1].x;

                //不算自己
                for (i = path.Count - 2; i >= 0; i--)
                {
                    Vector3Int tryPos = path[i];
                    //new Layer
                    if(tryPos.y!=y)
                    {
                        forward = back = tryPos.z;
                        right = left = tryPos.x;
                        y = tryPos.y;
                    }

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

                    if (!Check(checkLeft, checkRight, y, y, checkBack, checkForward))
                    {
                        Debug.Log("停在了"+ path[i + 1]);
                        return (nc.MapPos2RealPos(path[i + 1]) - cur).normalized;
                    }
                }

                if (i < 0)
                {
                    return (tar - cur).normalized;
                }
            }
            return (tar - cur).normalized;
        }

        public bool CanPass(Vector3Int tar)
        {
            return !visited.Contains(tar) && nc.InArea(tar) && nc.navUnits[tar.x, tar.y, tar.z].type == PassType.Can;
        }

        public bool Check(int startX, int endX, int startY, int endY, int startZ, int endZ)
        {
            for (int i = startX; i <= endX; i++)
                for (int j = startY; j <= endY; j++)
                    for (int k = startZ; k <= endZ; k++)
                    {
                        if (nc.navUnits[i, j, k].type != PassType.Can)
                        {
                            return false;
                        }
                    }

            return true;

        }
        public void DebugPath(List<Vector3Int> lst)
        {
            Debug.Log("now:");
            foreach (var o in lst)
                Debug.Log(o);
        }
    }
}