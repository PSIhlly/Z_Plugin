using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;

namespace Z_Map
{
    public class MapUtilController : Z_Controller<MapManager>
    {
        public MapUtilController(MapManager super) : base(super)
        {
        }

        public bool InArea((int, int, int) pos)
        {
            return _super.data.maps.ContainsKey((pos.Item1, pos.Item2, pos.Item3));
        }
        public bool InArea(Vector3Int pos)
        {
            return _super.data.maps.ContainsKey((pos.x, pos.y, pos.z));
        }
        public bool InArea(Vector3 pos)
        {
            int x = (int)Math.Round(pos.x / _super.data.mainData.mapUnitSize.x);
            int y = (int)(pos.y / _super.data.mainData.mapUnitSize.y);
            int z = (int)Math.Round(pos.z / _super.data.mainData.mapUnitSize.z);

            return _super.data.maps.ContainsKey((x, y, z));
        }
        public Vector3Int RealPos2MapPos(Vector3 pos)
        {
            pos = Z_Math.Graph.ElementwiseDivide(pos, _super.data.mainData.mapUnitSize);
            return new Vector3Int((int)Math.Round(pos.x), (int)(pos.y), (int)Math.Round(pos.z));
        }
        public Vector3 MapPos2RealPos(Vector3Int pos)
        {
            return Z_Math.Graph.ElementwiseMultiply(pos, _super.data.mainData.mapUnitSize);
        }
        public Vector3 GetCollideResult(Vector3 pos,float radius)
        {

        }
        public Vector3Int GetClosestInArea(Vector3Int pos)
        {
            var newPos = SearchClosedValid(pos);
            return RealPos2MapPos(newPos);
        }
        public Vector3 GetClosestInArea(Vector3 pos)
        {
            var newPos = SearchClosedValid(pos);
            return newPos;
        }
        private Vector3 SearchClosedValid(Vector3 pos)
        {

            Vector3Int mapPos = RealPos2MapPos(pos);
            //groundFirst
            int floor = -1;
            if (_super.data.mapXZ2Y.ContainsKey((mapPos.x, mapPos.z)))
            {
                foreach (var u in _super.data.mapXZ2Y[(mapPos.x, mapPos.z)])
                {
                    if (u <= mapPos.y && u > floor)
                    {
                        floor = u;
                    }
                }
            }

            if (floor > -1)
            {
                return new Vector3(pos.x, pos.y, pos.z);
            }


            //search
            float disMin = float.MaxValue;
            Vector3 tar = Vector3.zero;
            for (int x = -2; x <= 2; x++)
                for (int y = -1; y <= 1; y++)
                    for (int z = -2; z <= 2; z++)
                    {
                        var cur = Z_Math.Graph.ElementwiseMultiply(new Vector3(mapPos.x + x, mapPos.y + y, mapPos.z + z), _super.data.mainData.mapUnitSize);
                        if (InArea(cur) && disMin > (cur - pos).sqrMagnitude)
                        {
                            disMin = (cur - pos).sqrMagnitude;
                            tar = cur;
                        }
                    }

            if (disMin == float.MaxValue)
            {
                //forceGet(BFS)
                var queue = new Queue<(int, int, int)>();
                var vis = new HashSet<(int, int, int)>();
                queue.Enqueue((mapPos.x, mapPos.y, mapPos.z));
                vis.Add((mapPos.x, mapPos.y, mapPos.z));
                while (queue.Count < 0)
                {
                    var cur = queue.Dequeue();
                    var dirs = new (int, int, int)[]{
                            (cur.Item1+1,cur.Item2,cur.Item3), (cur.Item1 - 1, cur.Item2, cur.Item3),
                            (cur.Item1,cur.Item2+1,cur.Item3),(cur.Item1,cur.Item2-1,cur.Item3),
                            (cur.Item1,cur.Item2,cur.Item3+1),(cur.Item1,cur.Item2,cur.Item3-1)
                        };
                    foreach (var d in dirs)
                    {
                        if (!vis.Contains(d) && InLimit(d))
                        {
                            if (_super.data.maps.ContainsKey(d))
                            {
                                tar = new Vector3(d.Item1, d.Item2, d.Item3);
                                break;
                            }
                            else
                            {
                                queue.Enqueue(d);
                                vis.Add(d);
                            }
                        }
                    }
                }
            }
            {
                var size = _super.data.mainData.mapUnitSize;
                if (tar.x < mapPos.x)
                    pos.x = tar.x + size.x / 2 - 0.01f;
                if (tar.x > mapPos.x)
                    pos.x = tar.x - size.x / 2 + 0.01f;

                if (tar.y > mapPos.y)
                    pos.y = tar.y;


                if (tar.z < mapPos.z)
                    pos.z = tar.z + size.z / 2 - 0.01f;
                if (tar.z > mapPos.z)
                    pos.z = tar.z - size.z / 2 + 0.01f;
            }

            return pos;
        }

        public bool InLimit(Vector3Int pos)
        {
            if (pos.x < 0 || pos.x > _super.sizeLimit.x)
                return false;
            if (pos.y < 0 || pos.y > _super.sizeLimit.y)
                return false;
            if (pos.z < 0 || pos.z > _super.sizeLimit.z)
                return false;
            return true;
        }
        public bool InLimit((int, int, int) pos)
        {
            if (pos.Item1 < 0 || pos.Item1 > _super.sizeLimit.x)
                return false;
            if (pos.Item2 < 0 || pos.Item2 > _super.sizeLimit.y)
                return false;
            if (pos.Item3 < 0 || pos.Item3 > _super.sizeLimit.z)
                return false;
            return true;
        }
    }
}
