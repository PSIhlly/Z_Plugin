using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GPUDriven;
using Z_DesignStyle;
using Z_Map.Analysis;
using Z_Map.Form;
using Z_Math;
using Z_Mesh;
using Z_Ui;
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
        Vector3[] tryDir = new Vector3[] { Vector3.forward * 0.1f, Vector3.back * 0.1f, Vector3.left * 0.1f, Vector3.right * 0.1f };
        public MapUtilController(MapManager super) : base(super)
        {
        }
        public List<TileUnit> GetNineTile((int, int, int) mapPos, float length)
        {
            List<TileUnit> res = new List<TileUnit>();
            int area = (int)Math.Max(1, length);
            for (int i = mapPos.Item1 - area; i <= mapPos.Item1 + area; i++)
                for (int k = mapPos.Item2; k <= mapPos.Item2 + 1; k++)
                    for (int j = mapPos.Item3 - area; j <= mapPos.Item3 + area; j++)
                {
                    var tile = GetTile(i, k, j);
                    if (tile != null&& !res.Contains(tile))
                        res.Add(tile);
                }
            return res;
        }
        public bool InArea((int, int, int) pos)
        {
            return InArea(pos.Item1, pos.Item2, pos.Item3);
        }
        public bool InArea(Vector3Int pos)
        {
            return InArea(pos.x, pos.y, pos.z);
        }
        public bool InArea(Vector3 pos)
        {
            int x = (int)Math.Round(pos.x / _super.data.mainData.mapUnitSize.x);
            int y = (int)(pos.y / _super.data.mainData.mapUnitSize.y);
            int z = (int)Math.Round(pos.z / _super.data.mainData.mapUnitSize.z);

            return InArea(x, y, z);
        }
        /// <summary>
        /// 判断位置是否在地图区域内：当前y层有tile，或下方有tile（允许角色走到高层边界外再下落）
        /// </summary>
        private bool InArea(int x, int y, int z)
        {
            if (ContainsTile(x, y, z))
                return true;
            //当前y层没有tile时，下方有tile也算在区域内
            if (_super.data.mapXZ2Y.ContainsKey((x, z)))
            {
                foreach (var yLevel in _super.data.mapXZ2Y[(x, z)])
                {
                    if (yLevel < y)
                        return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 获取指定位置的TileUnit，如果当前y层没有tile则自动取下方最近的tile
        /// </summary>
        public TileUnit GetTile(int x, int y, int z)
        {
            if (_super.data.maps.ContainsKey((x, y, z)))
                return _super.data.maps[(x, y, z)].unit;
            //当前y层没有tile时，取下方最近的tile
            if (_super.data.mapXZ2Y.ContainsKey((x, z)))
            {
                int floorY = -1;
                foreach (var yLevel in _super.data.mapXZ2Y[(x, z)])
                {
                    if (yLevel < y && yLevel > floorY)
                        floorY = yLevel;
                }
                if (floorY > -1 && _super.data.maps.ContainsKey((x, floorY, z)))
                    return _super.data.maps[(x, floorY, z)].unit;
            }
            return null;
        }
        /// <summary>
        /// 获取指定位置的地图数据，仅当前y层有tile时返回（不自动往下取）
        /// </summary>
        public TileUnitForm.Data GetTileData(int x, int y, int z)
        {
            if (_super.data.maps.ContainsKey((x, y, z)))
                return _super.data.maps[(x, y, z)];
            return null;
        }
        /// <summary>
        /// 判断指定位置的当前y层是否存在tile（不自动往下取）
        /// </summary>
        public bool ContainsTile(int x, int y, int z)
        {
            return _super.data.maps.ContainsKey((x, y, z));
        }
        public bool IsOnBoundary(Vector3 pos)
        {
            if (_super.enable)
                for (int i = 0; i < tryDir.Length; i++)
                {
                    if (!InArea(pos + tryDir[i]))
                        return true;
                }
            return false;
        }

        public Vector3Int RealPos2MapPosInt(Vector3 pos)
        {
            if (_super.enable)
                pos = Z_Math.Graph.ElementwiseDivide(pos, _super.data.mainData.mapUnitSize);
            return new Vector3Int((int)Math.Round(pos.x), (int)Math.Round(pos.y), (int)Math.Round(pos.z));
        }
        public Vector3 RealPos2MapPos(Vector3 pos)
        {
            if (_super.enable)
                pos = Z_Math.Graph.ElementwiseDivide(pos, _super.data.mainData.mapUnitSize);
            return pos;
        }
        public Vector3 MapPos2RealPos(Vector3 pos)
        {
            if (_super.enable)
                return Z_Math.Graph.ElementwiseMultiply(pos, _super.data.mainData.mapUnitSize);
            return pos;
        }
        public Vector3 MapPos2RealPos(Vector3Int pos)
        {
            if (_super.enable)
                return Z_Math.Graph.ElementwiseMultiply(pos, _super.data.mainData.mapUnitSize);
            else
                return pos;
        }

        public Vector3Int GetClosestInArea(Vector3Int pos)
        {
            Vector3 newPos = pos;
            if (_super.enable)
            {
                newPos = SearchClosedValid(MapPos2RealPos(pos));
            }
            return RealPos2MapPosInt(newPos);
        }
        public Vector3 GetClosestInArea(Vector3 pos)
        {
            var newPos = pos;
            if (_super.enable)
                newPos = SearchClosedValid(pos);
            return newPos;
        }
        private Vector3 SearchClosedValid(Vector3 pos)
        {

            Vector3Int mapPos = RealPos2MapPosInt(pos);
            //groundFirst
            int floor = -1;
            if (GlobalSettings.ENABLE_GRAVITY)
            {
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

            }
            else
            {
                if (ContainsTile(mapPos.x, mapPos.y, mapPos.z))
                {
                    floor = 1;
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
                while (queue.Count > 0)
                {
                    var cur = queue.Dequeue();
                    var dirs = new (int, int, int)[]{
                            (cur.Item1+1,cur.Item2,cur.Item3), (cur.Item1 - 1, cur.Item2, cur.Item3),
                            (cur.Item1,cur.Item2,cur.Item3+1),(cur.Item1,cur.Item2,cur.Item3-1)
                        };
                    bool finded = false;
                    foreach (var d in dirs)
                    {
                        if (!vis.Contains(d) && InLimit(d))
                        {
                            if (ContainsTile(d.Item1, d.Item2, d.Item3))
                            {
                                tar = new Vector3(d.Item1, d.Item2, d.Item3);
                                finded = true;
                                break;
                            }
                            else
                            {
                                queue.Enqueue(d);
                                vis.Add(d);
                            }
                        }
                    }
                    if (finded)
                        break;
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
            //计算root的世界旋转的逆，用于把子物体的世界旋转转换到root局部坐标系
            Quaternion rootRotInv = Quaternion.Inverse(root.transform.rotation);
            Quaternion rootRotFinal = Quaternion.Euler(rootEuler);
            //prefab根的lossyScale，用于把collider的lossyScale换算成“相对root的局部scale”，
            //避免与rootScale相乘时重复计入prefab根的缩放
            Vector3 rootLossyScale = root.transform.lossyScale;
            foreach (var c in cs)
            {
                if (type == CollideType.CollideOnly && c.isTrigger)
                    continue;
                if (type == CollideType.TriggerOnly && !c.isTrigger)
                    continue;
                //位置变换：用InverseTransformPoint把子物体世界位置转换到root局部空间（已抵消root的旋转和缩放），
                //再用rootScale重新缩放、rootRotFinal重新旋转，最后叠加rootPos。
                //旧实现直接用rootRotInv*(pos-rootPos)得到的是世界单位偏移，未按rootScale重新缩放，
                //当rootScale!=(1,1,1)且有嵌套collider时位置会偏。
                Vector3 localPosInRoot = root.transform.InverseTransformPoint(c.transform.position);
                Vector3 finalPos = rootPos + rootRotFinal * Graph.ElementwiseMultiply(localPosInRoot, rootScale);
                //旋转变换：把子物体世界旋转转换到root局部空间，再用rootEuler旋转
                Quaternion childLocalRot = rootRotInv * c.transform.rotation;
                Vector3 finalEuler = (rootRotFinal * childLocalRot).eulerAngles;
                //scale变换：collider的lossyScale已包含prefab根的scale，需先除掉根scale再用rootScale重新缩放，
                //否则prefab根scale非1时会与rootScale重复相乘
                Vector3 finalScale = Graph.ElementwiseMultiply(Graph.ElementwiseDivide(c.transform.lossyScale, rootLossyScale), rootScale);
                if (c is BoxCollider box)
                {
                    res.Add(Mesh.GetMesh(box, finalPos, finalEuler, finalScale));
                }
                else if (c is SphereCollider sp)
                {
                    res.Add(Mesh.GetMesh(sp, finalPos, finalEuler, finalScale));
                }
            }
            return res;
        }
        public List<MapUnit> CaptureCast(Vector3 from, Vector3 to, float radius)
        {
            var res = new List<MapUnit>();

            HashSet<int> exist = new HashSet<int>();

            var fromMesh = Mesh.GetMesh(from, radius, Vector3.zero, Vector3.one);
            var toMesh = Mesh.GetMesh(to, radius, Vector3.zero, Vector3.one);
            var lst = Graph.GetRoughOverlapIntPos(fromMesh.positions);
            lst.AddRange(Graph.GetRoughOverlapIntPos(toMesh.positions));


            foreach (var pos in lst)
            {
                var mapPos = _super.utilCtrl.RealPos2MapPosInt(pos);
                var tile = GetTile(mapPos.x, mapPos.y, mapPos.z);
                if (tile != null)
                {
                    var unitLst = new List<MapUnit>() { tile };
                    unitLst.AddRange(_super.updateCtrl.objectTileDic.Get(tile));
                    unitLst.AddRange(_super.updateCtrl.characterTileDic.Get(tile));
                    foreach (var u in unitLst)
                    {
                        if (exist.Contains(u.data.uid))
                            continue;
                        exist.Add(u.data.uid);

                        _super.updateCtrl.CheckCollide(fromMesh, u, to - from, CollideType.CollideOnly, out _, out var assist);
                        if (assist.GetRes() != Graph.IntersectType.None)
                        {
                            res.Add(u);
                        }

                    }

                }
                Debug.DrawLine(from, to, Color.green);
            }
            return res;
        }
        public List<TileUnit> GetOverlap(ObjectUnitForm.Data oData)
        {
            var all = new List<List<Vector3Int>>();
            foreach (var c in oData.unit.GetMeshes(CollideType.CollideOnly))
            {
                all.Add(Graph.GetRoughOverlapIntPos(c.positions));
            }
            var res = Graph.DeduplicateIntPos(all);
            _super.updateCtrl.objectTileDic.Del(oData.unit);
            var ans = new List<TileUnit>();
            foreach (var p in res)
            {
                var mp = RealPos2MapPosInt(p);
                if (InArea(mp))
                {
                    var t = GetTile(mp.x, mp.y, mp.z);
                    if (t != null)
                        ans.Add(t);
                }
            }

            return ans;
        }
        public void SetPerspectiveModel(Transform rootTrs, Transform imgTrs, float deepth)
        {
            switch (DynamicGlobalSettings.cameraMode)
            {
                case CameraMode.Overhead:
                    imgTrs.SetParent(rootTrs);
                    imgTrs.position = rootTrs.position + Vector3.up * rootTrs.localScale.y / 2 + Vector3.down * deepth;
                    imgTrs.localScale = Vector3.one;
                    break;
                case CameraMode.Isometric:
                    // 直接由已知信息推算 imgTrs 的变换，无需中间节点。
                    imgTrs.SetParent(rootTrs);
                    imgTrs.position = rootTrs.position + new Vector3(0, 0.207f, 0) + new Vector3(0, -1, -1) * deepth;
                    imgTrs.rotation = Quaternion.identity;
                    imgTrs.localScale = new Vector3(rootTrs.localScale.x, rootTrs.localScale.y * 1.414f, rootTrs.localScale.z);
                    break;
            }
        }
    }
}
