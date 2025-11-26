using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using Z_DesignStyle;
using Z_Map.Form;
using Z_Math;
using Z_Mesh;
using static UnityEditor.PlayerSettings;
using Mesh = Z_Mesh.Mesh;
namespace Z_Map
{
    public enum CollideType
    {
        All,
        TriggerOnly,
        CollideOnly,
    }
    public class MapUtilController : Z_Controller<MapManager>
    {
        public MapUtilController(MapManager super) : base(super)
        {
        }
        public List<TileUnit> GetNineTile((int, int, int) mapPos)
        {
            List<TileUnit> res = new List<TileUnit>();
            for (int i = mapPos.Item1 - 1; i <= mapPos.Item1 + 1; i++)
                for (int j = mapPos.Item3 - 1; j <= mapPos.Item3 + 1; j++)
                {
                    if (_super.data.maps.ContainsKey((i, mapPos.Item2, j)))
                    {
                        res.Add(_super.data.maps[(i, mapPos.Item2, j)].unit);
                    }
                }
            return res;
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
        public List<MeshInfo> GetCollidersMesh(GameObject root, Vector3 rootPos, Vector3 rootEuler, Vector3 rootScale, CollideType type = CollideType.All)
        {
            UnityEngine.Collider[] cs = root.GetComponentsInChildren<Collider>();
            var res = new List<MeshInfo>();
            foreach (var c in cs)
            {
                if (type == CollideType.CollideOnly && c.isTrigger)
                    continue;
                if (type == CollideType.TriggerOnly && !c.isTrigger)
                    continue;
                var pos = c.transform.position;
                if (c is BoxCollider box)
                {
                    res.Add(Mesh.GetMesh(box, pos - root.transform.position + rootPos, rootEuler, rootScale));
                }
                else if (c is SphereCollider sp)
                {
                    res.Add(Mesh.GetMesh(sp, pos - root.transform.position + rootPos, rootEuler, rootScale));
                }
            }
            return res;
        }
        public List<TileUnit> GetOverlap(ObjectUnitForm.Data oData)
        {
            var all = new List<List<Vector3Int>>();
            foreach (var c in GetCollidersMesh(oData.unit.prefab, oData.pos, oData.euler, oData.scale, CollideType.CollideOnly))
            {
                all.Add(Graph.GetRoughOverlapIntPos(c.positions));
            }
            var res = Graph.DeduplicateIntPos(all);
            _super.updateCtrl.objectTileDic.Del(oData.unit);
            var ans = new List<TileUnit>();
            foreach (var p in res)
            {
                var mp = RealPos2MapPos(p);
                if (InArea(mp))
                {
                    ans.Add(_super.data.maps[(mp.x, mp.y, mp.z)].unit);
                }
            }

            return ans;
        }
        public void SetPerspectiveModel(Transform rootTrs, Transform imgTrs, float deepth)
        {
            switch (DynamicGlobalSettings.cameraMode)
            {
                case CameraMode.Overhead:
                    imgTrs.position = rootTrs.position+ Vector3.up * rootTrs.localScale.y / 2 + Vector3.down * deepth;
                    imgTrs.localScale = Vector3.one;
                    break;
                case CameraMode.Isometric:
                    imgTrs.position = rootTrs.position + new Vector3(0, -1, -1) * deepth;
                    Graph.CalculateTriangle(rootTrs.localScale.z, rootTrs.localScale.y, out var y, out var angle);
                    imgTrs.localScale = new Vector3(rootTrs.localScale.x, y, rootTrs.localScale.z);
                    imgTrs.eulerAngles = new Vector3(angle, 0, 0);
                    break;
            }
        }
    }
}
