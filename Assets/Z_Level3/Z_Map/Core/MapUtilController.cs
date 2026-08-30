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
            return InArea(pos, 0.2f);
        }
        /// <summary>
        /// 判断真实位置是否位于地图区域内。boundaryDistance为水平外边界内缩距离；
        /// 传0时按单位中心是否真正触到或越过地图边缘判断，不保留固定边距。
        /// </summary>
        public bool InArea(Vector3 pos, float boundaryDistance)
        {
            int x = (int)Math.Round(pos.x / _super.data.mainData.mapUnitSize.x);
            int y = (int)(pos.y / _super.data.mainData.mapUnitSize.y);
            int z = (int)Math.Round(pos.z / _super.data.mainData.mapUnitSize.z);

            // 距水平外边界指定距离以内时视为不在区域；Object移动传0，不使用固定内缩距离。
            if (IsNearBoundaryEdge(pos, x, y, z, boundaryDistance))
                return false;

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
        /// <summary>
        /// 检查pos是否在指定地图边缘距离以内。会沿水平连续可行走tile查找真实外边界，
        /// 因此boundaryDistance大于单格尺寸时仍能正确判定。
        /// InArea(Vector3)使用基础距离0.2；InArea(Vector3,float)和IsOnBoundary可由调用方指定距离。
        /// </summary>
        private bool IsNearBoundaryEdge(Vector3 pos, int x, int y, int z, float boundaryDistance)
        {
            boundaryDistance = Mathf.Max(0f, boundaryDistance);
            var size = GetSafeMapUnitSize();
            const float tolerance = 0.0001f;
            int xSteps = Mathf.CeilToInt(boundaryDistance / size.x) + 1;
            int zSteps = Mathf.CeilToInt(boundaryDistance / size.z) + 1;

            if (TryGetHorizontalBoundary(x, y, z, 1, 0, xSteps, size, out float right)
                && right - pos.x <= boundaryDistance + tolerance)
                return true;
            if (TryGetHorizontalBoundary(x, y, z, -1, 0, xSteps, size, out float left)
                && pos.x - left <= boundaryDistance + tolerance)
                return true;
            if (TryGetHorizontalBoundary(x, y, z, 0, 1, zSteps, size, out float forward)
                && forward - pos.z <= boundaryDistance + tolerance)
                return true;
            if (TryGetHorizontalBoundary(x, y, z, 0, -1, zSteps, size, out float back)
                && pos.z - back <= boundaryDistance + tolerance)
                return true;
            return false;
        }

        /// <summary>
        /// 将非传送角色的目标位置限制在地图水平外边界以内。
        /// fromPos用于目标位置已经跨入空白格时定位角色原本所属的连续地图区域。
        /// </summary>
        public Vector3 ClampMoveToAreaBoundary(Vector3 fromPos, Vector3 targetPos, float boundaryDistance)
        {
            if (!_super.enable || boundaryDistance <= 0f)
                return targetPos;

            Vector3Int reference = RealPos2MapPosInt(targetPos);
            if (!InArea(reference))
            {
                reference = RealPos2MapPosInt(fromPos);
                if (!InArea(reference))
                    return targetPos;
            }

            var size = GetSafeMapUnitSize();
            int xSteps = Mathf.CeilToInt((Mathf.Abs(targetPos.x - fromPos.x) + boundaryDistance) / size.x) + 1;
            int zSteps = Mathf.CeilToInt((Mathf.Abs(targetPos.z - fromPos.z) + boundaryDistance) / size.z) + 1;

            float minX = float.NegativeInfinity;
            float maxX = float.PositiveInfinity;
            if (TryGetHorizontalBoundary(reference.x, reference.y, reference.z, -1, 0, xSteps, size, out float left))
                minX = left + boundaryDistance;
            if (TryGetHorizontalBoundary(reference.x, reference.y, reference.z, 1, 0, xSteps, size, out float right))
                maxX = right - boundaryDistance;
            targetPos.x = ClampToBoundaryRange(targetPos.x, minX, maxX);

            float minZ = float.NegativeInfinity;
            float maxZ = float.PositiveInfinity;
            if (TryGetHorizontalBoundary(reference.x, reference.y, reference.z, 0, -1, zSteps, size, out float back))
                minZ = back + boundaryDistance;
            if (TryGetHorizontalBoundary(reference.x, reference.y, reference.z, 0, 1, zSteps, size, out float forward))
                maxZ = forward - boundaryDistance;
            targetPos.z = ClampToBoundaryRange(targetPos.z, minZ, maxZ);

            return targetPos;
        }

        private Vector3 GetSafeMapUnitSize()
        {
            var size = _super.data.mainData.mapUnitSize;
            size.x = Mathf.Max(0.0001f, Mathf.Abs(size.x));
            size.y = Mathf.Max(0.0001f, Mathf.Abs(size.y));
            size.z = Mathf.Max(0.0001f, Mathf.Abs(size.z));
            return size;
        }

        private bool TryGetHorizontalBoundary(int x, int y, int z, int stepX, int stepZ, int maxSteps, Vector3 size, out float boundary)
        {
            for (int step = 1; step <= maxSteps; step++)
            {
                int nextX = x + stepX * step;
                int nextZ = z + stepZ * step;
                if (InArea(nextX, y, nextZ))
                    continue;

                if (stepX > 0)
                    boundary = (nextX - 0.5f) * size.x;
                else if (stepX < 0)
                    boundary = (nextX + 0.5f) * size.x;
                else if (stepZ > 0)
                    boundary = (nextZ - 0.5f) * size.z;
                else
                    boundary = (nextZ + 0.5f) * size.z;
                return true;
            }

            boundary = 0f;
            return false;
        }

        private static float ClampToBoundaryRange(float value, float min, float max)
        {
            // 当地图在这一轴上的宽度小于两倍缩进时不存在正常区间，固定在区域中点。
            if (min > max)
                return (min + max) * 0.5f;
            return Mathf.Clamp(value, min, max);
        }

        public bool IsOnBoundary(Vector3 pos, float boundaryDistance = 0.2f)
        {
            if (!_super.enable)
                return false;
            int x = (int)Math.Round(pos.x / _super.data.mainData.mapUnitSize.x);
            int y = (int)(pos.y / _super.data.mainData.mapUnitSize.y);
            int z = (int)Math.Round(pos.z / _super.data.mainData.mapUnitSize.z);
            return IsNearBoundaryEdge(pos, x, y, z, boundaryDistance);
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

        public Vector3Int GetClosestExistInArea(Vector3Int pos)
        {
            Vector3 newPos = pos;
            if (_super.enable)
            {
                newPos = SearchClosedExist(MapPos2RealPos(pos));
            }
            return RealPos2MapPosInt(newPos);
        }
        public Vector3 GetClosestInArea(Vector3 pos)
        {
            var newPos = pos;
            if (_super.enable)
                newPos = SearchClosedExist(pos);
            return newPos;
        }
        private Vector3 SearchClosedExist(Vector3 pos)
        {

            Vector3Int mapPos = RealPos2MapPosInt(pos);
            if (ContainsTile(mapPos.x, mapPos.y, mapPos.z))
                return pos;
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
                return new Vector3(pos.x,  floor* _super.data.mainData.mapUnitSize.y, pos.z);
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
                int deepth = 100;
                while (queue.Count > 0&& deepth>0)
                {
                    deepth++;
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
                    unitLst.AddRange(_super.updateCtrl.characterOverlapTileDic.Get(tile));
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
        public List<TileUnit> GetCharacterCollisionTiles(CharacterUnit unit, Vector3 displacement, CollideType type)
        {
            var result = new HashSet<TileUnit>();
            bool hasPoint = false;
            Vector3 min = Vector3.zero;
            Vector3 max = Vector3.zero;

            foreach (var mesh in unit.GetMeshes(type))
            {
                if (mesh.positions == null)
                    continue;
                foreach (var point in mesh.positions)
                {
                    Vector3 movedPoint = point + displacement;
                    if (!hasPoint)
                    {
                        min = Vector3.Min(point, movedPoint);
                        max = Vector3.Max(point, movedPoint);
                        hasPoint = true;
                    }
                    else
                    {
                        min = Vector3.Min(min, Vector3.Min(point, movedPoint));
                        max = Vector3.Max(max, Vector3.Max(point, movedPoint));
                    }
                }
            }

            if (!hasPoint)
            {
                min = Vector3.Min(unit.data.pos, unit.data.pos + displacement);
                max = Vector3.Max(unit.data.pos, unit.data.pos + displacement);
            }

            Vector3 cellSize = _super.enable ? _super.data.mainData.mapUnitSize : Vector3.one;
            cellSize.x = Mathf.Max(0.0001f, Mathf.Abs(cellSize.x));
            cellSize.y = Mathf.Max(0.0001f, Mathf.Abs(cellSize.y));
            cellSize.z = Mathf.Max(0.0001f, Mathf.Abs(cellSize.z));

            int minX = Mathf.FloorToInt(min.x / cellSize.x - 0.5f);
            int maxX = Mathf.CeilToInt(max.x / cellSize.x + 0.5f);
            int minZ = Mathf.FloorToInt(min.z / cellSize.z - 0.5f);
            int maxZ = Mathf.CeilToInt(max.z / cellSize.z + 0.5f);
            int anchorY = unit.belongTile != null
                ? unit.belongTile.data.mapPos.y
                : RealPos2MapPosInt(unit.data.pos).y;
            int maxY = Mathf.Max(anchorY + 1, Mathf.CeilToInt(max.y / cellSize.y + 0.5f));

            for (int x = minX; x <= maxX; x++)
            for (int y = anchorY; y <= maxY; y++)
            for (int z = minZ; z <= maxZ; z++)
            {
                var tile = GetTile(x, y, z);
                if (tile != null)
                    result.Add(tile);
            }

            return new List<TileUnit>(result);
        }
        public List<TileUnit> GetColliderOverlap(ObjectUnitForm.Data oData)
        {
            var all = new List<List<Vector3Int>>();
            foreach (var c in oData.unit.GetMeshes(CollideType.All))
            {
                all.Add(Graph.GetRoughOverlapIntPos(c.positions));
            }
            var res = Graph.DeduplicateIntPos(all);
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

        public List<TileUnit> GetVisionOverlap(ObjectUnitForm.Data oData)
        {
            var result = new List<TileUnit>();
            var added = new HashSet<TileUnit>();
            TryGetVisionBounds(oData, out Bounds visionBounds);
            Vector3 min = RealPos2MapPos(visionBounds.min);
            Vector3 max = RealPos2MapPos(visionBounds.max);

            // 先加入锚点，使 ObjectUnit.belongTile 的 GetFirst 语义保持稳定。
            Vector3Int anchor = RealPos2MapPosInt(oData.pos);
            AddVisionOverlapTile(anchor.x, anchor.y, anchor.z, added, result);

            int minX = Mathf.FloorToInt(min.x - 0.5f) + 1;
            int maxX = Mathf.CeilToInt(max.x + 0.5f) - 1;
            int minY = Mathf.FloorToInt(min.y - 0.5f) + 1;
            int maxY = Mathf.CeilToInt(max.y + 0.5f) - 1;
            int minZ = Mathf.FloorToInt(min.z - 0.5f) + 1;
            int maxZ = Mathf.CeilToInt(max.z + 0.5f) - 1;

            for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
            for (int z = minZ; z <= maxZ; z++)
                AddVisionOverlapTile(x, y, z, added, result);

            return result;
        }

        public bool TryGetVisionBounds(ObjectUnitForm.Data oData, out Bounds visionBounds)
        {
            visionBounds = default;
            bool hasBounds = false;
            GameObject root = oData.unit.prefab;
            if (root != null)
            {
                Matrix4x4 rootToRuntime = Matrix4x4.TRS(
                    oData.pos,
                    Quaternion.Euler(oData.euler),
                    oData.scale);

                // GameSample 运行时 Object prefab 的直接子节点保存了 MapModel 的 pos/scale。
                // CombineNewGoByPrefabs 会给 localPosition 额外加 0.5Y，因此先还原模型底部，
                // 再按视觉高度求中心。该路径不受 PerspectiveKeeper 或 colliderScale 影响。
                if (root.GetComponent<ObjectInstance>() != null)
                {
                    for (int i = 0; i < root.transform.childCount; i++)
                    {
                        Transform child = root.transform.GetChild(i);
                        Vector3 visualScale = new Vector3(
                            Mathf.Abs(child.localScale.x),
                            Mathf.Abs(child.localScale.y),
                            Mathf.Abs(child.localScale.z));
                        Vector3 visualBase = child.localPosition - Vector3.up * 0.5f;
                        Vector3 visualCenter = visualBase + Vector3.up * (visualScale.y * 0.5f);
                        Matrix4x4 visualToRuntime = rootToRuntime
                            * Matrix4x4.TRS(visualCenter, child.localRotation, visualScale);

                        for (int x = 0; x < 2; x++)
                        for (int y = 0; y < 2; y++)
                        for (int z = 0; z < 2; z++)
                        {
                            Vector3 worldCorner = visualToRuntime.MultiplyPoint3x4(new Vector3(
                                x == 0 ? -0.5f : 0.5f,
                                y == 0 ? -0.5f : 0.5f,
                                z == 0 ? -0.5f : 0.5f));
                            if (hasBounds)
                                visionBounds.Encapsulate(worldCorner);
                            else
                            {
                                visionBounds = new Bounds(worldCorner, Vector3.zero);
                                hasBounds = true;
                            }
                        }
                    }

                    if (hasBounds)
                        return true;
                }

                Matrix4x4 prefabWorldToRoot = root.transform.worldToLocalMatrix;

                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    if (!renderer.enabled
                        || renderer.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly)
                        continue;

                    Matrix4x4 rendererToRuntime = rootToRuntime
                        * prefabWorldToRoot
                        * renderer.transform.localToWorldMatrix;
                    Bounds localBounds = renderer.localBounds;
                    Vector3 localMin = localBounds.min;
                    Vector3 localMax = localBounds.max;

                    for (int x = 0; x < 2; x++)
                    for (int y = 0; y < 2; y++)
                    for (int z = 0; z < 2; z++)
                    {
                        Vector3 localCorner = new Vector3(
                            x == 0 ? localMin.x : localMax.x,
                            y == 0 ? localMin.y : localMax.y,
                            z == 0 ? localMin.z : localMax.z);
                        Vector3 worldCorner = rendererToRuntime.MultiplyPoint3x4(localCorner);
                        if (hasBounds)
                            visionBounds.Encapsulate(worldCorner);
                        else
                        {
                            visionBounds = new Bounds(worldCorner, Vector3.zero);
                            hasBounds = true;
                        }
                    }
                }
            }

            if (hasBounds)
                return true;

            // 无可视 Renderer 时保留锚点盒，避免 Object 从空间索引中消失。
            Vector3 scale = new Vector3(
                Mathf.Abs(oData.scale.x),
                Mathf.Abs(oData.scale.y),
                Mathf.Abs(oData.scale.z));
            Quaternion rotation = Quaternion.Euler(oData.euler);
            Vector3 center = oData.pos + rotation * new Vector3(0f, scale.y * 0.5f, 0f);
            Vector3[] corners = Graph.GetCubeEightPoint(center, scale, oData.euler);
            visionBounds = new Bounds(corners[0], Vector3.zero);
            for (int i = 1; i < corners.Length; i++)
                visionBounds.Encapsulate(corners[i]);
            return false;
        }

        public float GetVisionHeightInTiles(ObjectUnitForm.Data oData, int layerY)
        {
            TryGetVisionBounds(oData, out Bounds visionBounds);
            float layerBaseY = MapPos2RealPos(new Vector3(0, layerY, 0)).y;
            float cellHeight = _super.enable
                ? Mathf.Max(0.0001f, Mathf.Abs(_super.data.mainData.mapUnitSize.y))
                : 1f;
            return (visionBounds.max.y - layerBaseY) / cellHeight;
        }

        private void AddVisionOverlapTile(
            int x,
            int y,
            int z,
            HashSet<TileUnit> added,
            List<TileUnit> result)
        {
            TileUnit tile = GetTile(x, y, z);
            if (tile != null && added.Add(tile))
                result.Add(tile);
        }
    }
}
