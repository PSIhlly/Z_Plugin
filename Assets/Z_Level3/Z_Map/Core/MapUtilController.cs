using System;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;
using Z_Map.Analysis;
using Z_Map.Form;
using Z_Math;
using Z_Mesh;
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
        private const float ObjectTileProbeHeight = 1f;

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
        /// 放置归属沿用地图坐标换算，从目标层向下查找最近Tile。
        /// 只确定belong，不改变单位实际放置高度。
        /// </summary>
        public TileUnit GetPlacementTile(Vector3 position)
        {
            Vector3Int mapPos = RealPos2MapPosInt(position);
            return GetTile(mapPos.x, mapPos.y, mapPos.z);
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
                newPos = SearchClosestExist(MapPos2RealPos(pos));
            }
            return RealPos2MapPosInt(newPos);
        }
        public Vector3 GetClosestInArea(Vector3 pos)
        {
            var newPos = pos;
            if (_super.enable)
                newPos = SearchClosestExist(pos);
            return newPos;
        }
        public Vector3 GetClosestInArea(Vector3 pos, IReadOnlyCollection<int> passTypes)
        {
            var newPos = pos;
            if (_super.enable)
                newPos = SearchClosestExist(pos, passTypes);
            return newPos;
        }

        private Vector3 SearchClosestExist(Vector3 pos, IReadOnlyCollection<int> passTypes = null)
        {
            Vector3Int mapPos = RealPos2MapPosInt(pos);
            TileUnit exactTile = GetTileData(mapPos.x, mapPos.y, mapPos.z)?.unit;
            if (CanUseTile(exactTile, passTypes))
                return pos;

            //groundFirst
            int floor = -1;
            if (GlobalSettings.ENABLE_GRAVITY)
            {
                if (_super.data.mapXZ2Y.ContainsKey((mapPos.x, mapPos.z)))
                {
                    foreach (var u in _super.data.mapXZ2Y[(mapPos.x, mapPos.z)])
                    {
                        TileUnit floorTile = GetTileData(mapPos.x, u, mapPos.z)?.unit;
                        if (u <= mapPos.y && u > floor && CanUseTile(floorTile, passTypes))
                        {
                            floor = u;
                        }
                    }
                }
            }

            if (floor > -1)
            {
                return new Vector3(pos.x,  floor* _super.data.mainData.mapUnitSize.y, pos.z);
            }


            //search
            float disMin = float.MaxValue;
            Vector3 tar = Vector3.zero;
            bool found = false;
            for (int x = -2; x <= 2; x++)
                for (int y = -1; y <= 1; y++)
                    for (int z = -2; z <= 2; z++)
                    {
                        var candidatePos = new Vector3Int(mapPos.x + x, mapPos.y + y, mapPos.z + z);
                        TileUnit candidate = GetTile(candidatePos.x, candidatePos.y, candidatePos.z);
                        if (!CanUseTile(candidate, passTypes))
                            continue;

                        Vector3 cur = MapPos2RealPos(candidate.data.mapPos);
                        Vector3 closest = GetClosestPositionInTile(pos, cur);
                        float distance = (closest - pos).sqrMagnitude;
                        if (disMin > distance)
                        {
                            disMin = distance;
                            tar = cur;
                            found = true;
                        }
                    }

            if (!found)
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
                            TileUnit candidate = GetTile(d.Item1, d.Item2, d.Item3);
                            if (CanUseTile(candidate, passTypes))
                            {
                                tar = MapPos2RealPos(candidate.data.mapPos);
                                finded = true;
                                found = true;
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

            return found ? GetClosestPositionInTile(pos, tar) : pos;
        }

        private Vector3 GetClosestPositionInTile(Vector3 pos, Vector3 tileCenter)
        {
            Vector3 size = _super.data.mainData.mapUnitSize;
            float insetX = Mathf.Min(0.01f, Mathf.Abs(size.x) * 0.25f);
            float insetZ = Mathf.Min(0.01f, Mathf.Abs(size.z) * 0.25f);
            float halfX = Mathf.Abs(size.x) * 0.5f;
            float halfZ = Mathf.Abs(size.z) * 0.5f;
            pos.x = Mathf.Clamp(pos.x, tileCenter.x - halfX + insetX, tileCenter.x + halfX - insetX);
            pos.z = Mathf.Clamp(pos.z, tileCenter.z - halfZ + insetZ, tileCenter.z + halfZ - insetZ);
            if (tileCenter.y > pos.y)
                pos.y = tileCenter.y;
            return pos;
        }

        private static bool CanUseTile(TileUnit tile, IReadOnlyCollection<int> passTypes)
        {
            if (tile == null)
                return false;
            if (passTypes == null)
                return true;

            foreach (int requiredType in tile.passTypes)
            {
                bool contains = false;
                foreach (int passType in passTypes)
                {
                    if (passType == requiredType)
                    {
                        contains = true;
                        break;
                    }
                }
                if (!contains)
                    return false;
            }
            return true;
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
        // Read the original pool body every time; never shrink an already fitted
        // live Collider when a WangTile mask changes or a pooled Instance returns.
        public static void GetHorizontalBoundsBox(BoxCollider box, Rect bounds, float colliderScale,
            out Vector3 size, out Vector3 center)
        {
            var basis = box;
            if (box.isTrigger)
                foreach (var body in box.GetComponents<BoxCollider>())
                    if (body.enabled && !body.isTrigger && body.center == box.center)
                    {
                        basis = body;
                        break;
                    }
            GetTextureBoundsBox(basis.size, box.size, box.center, bounds, colliderScale,
                box.transform.localScale.y, out size, out center);
        }

        public static void GetTextureBoundsBox(Vector3 bodySize, Vector3 originalSize, Vector3 originalCenter,
            Rect bounds, float colliderScale, float colliderYScale, out Vector3 size, out Vector3 center)
        {
            // The image's vertical axis spans the cube's Y/Z diagonal. A fraction
            // h of that diagonal spans Y*h and Z*h, retaining the authored aspect
            // ratio after model/root scaling, without weighting that ratio twice.
            // Automatic Triggers keep their original padding on all three axes.
            size.x = bounds.width > 0 ? bodySize.x * bounds.width + (originalSize.x - bodySize.x) : 0;
            size.y = bounds.height > 0 ? bodySize.y * bounds.height + (originalSize.y - bodySize.y) : 0;
            size.z = bounds.height > 0 ? bodySize.z * bounds.height + (originalSize.z - bodySize.z) : 0;
            center = originalCenter;
            float verticalOffset = bounds.center.y - .5f;
            if (Mathf.Abs(colliderScale) > .000001f)
            {
                center.x += bodySize.x * (bounds.center.x - .5f) / colliderScale;
                center.z += bodySize.z * verticalOffset / colliderScale;
            }
            // Boxes leave Collider Y scale at one; converted Spheres may shrink
            // it too. In both cases move the center in the unshrunk image space.
            if (Mathf.Abs(colliderYScale) > .000001f)
                center.y += bodySize.y * verticalOffset / colliderYScale;
        }

        public List<MeshInfo> GetCollidersMesh(GameObject root, Vector3 rootPos, Vector3 rootEuler, Vector3 rootScale,
            CollideType type = CollideType.All, Rect? firstPartBounds = null, float boundsColliderScale = 1f,
            bool centerCollider = true)
        {
            // Runtime combined prefab roots are inactive pool templates. Ignore
            // that root state, but retain authored inactive-child exclusions.
            UnityEngine.Collider[] cs = root.GetComponentsInChildren<Collider>(true);
            var res = new List<MeshInfo>();
            //计算root的世界旋转的逆，用于把子物体的世界旋转转换到root局部坐标系
            Quaternion rootRotInv = Quaternion.Inverse(root.transform.rotation);
            Quaternion rootRotFinal = Quaternion.Euler(rootEuler);
            //prefab根的lossyScale，用于把collider的lossyScale换算成“相对root的局部scale”，
            //避免与rootScale相乘时重复计入prefab根的缩放
            Vector3 rootLossyScale = root.transform.lossyScale;
            Vector3 rearOffset = centerCollider ? Vector3.zero
                : rootRotFinal * GetObjectBackColliderOffset(root, firstPartBounds, boundsColliderScale, rootScale);
            foreach (var c in cs)
            {
                // Bounds-fitted Sphere components can await deferred destruction.
                if (!c.enabled)
                    continue;
                bool childActive = true;
                for (var t = c.transform; t != root.transform; t = t.parent)
                    if (!t.gameObject.activeSelf)
                    {
                        childActive = false;
                        break;
                    }
                if (!childActive)
                    continue;
                if (type == CollideType.CollideOnly && c.isTrigger)
                    continue;
                if (type == CollideType.TriggerOnly && !c.isTrigger)
                    continue;
                //位置变换：用InverseTransformPoint把子物体世界位置转换到root局部空间（已抵消root的旋转和缩放），
                //再用rootScale重新缩放、rootRotFinal重新旋转，最后叠加rootPos。
                //旧实现直接用rootRotInv*(pos-rootPos)得到的是世界单位偏移，未按rootScale重新缩放，
                //当rootScale!=(1,1,1)且有嵌套collider时位置会偏。
                Vector3 localPosInRoot = root.transform.InverseTransformPoint(c.transform.position);
                Vector3 finalPos = rootPos + rootRotFinal * Graph.ElementwiseMultiply(localPosInRoot, rootScale) + rearOffset;
                //旋转变换：把子物体世界旋转转换到root局部空间，再用rootEuler旋转
                Quaternion childLocalRot = rootRotInv * c.transform.rotation;
                Vector3 finalEuler = (rootRotFinal * childLocalRot).eulerAngles;
                //scale变换：collider的lossyScale已包含prefab根的scale，需先除掉根scale再用rootScale重新缩放，
                //否则prefab根scale非1时会与rootScale重复相乘
                Vector3 finalScale = Graph.ElementwiseMultiply(Graph.ElementwiseDivide(c.transform.lossyScale, rootLossyScale), rootScale);
                if (c is BoxCollider box)
                {
                    if (firstPartBounds.HasValue && root.transform.childCount > 0 &&
                        box.transform.IsChildOf(root.transform.GetChild(0)))
                    {
                        GetHorizontalBoundsBox(box, firstPartBounds.Value, boundsColliderScale, out var size, out var center);
                        res.Add(Mesh.GetMesh(finalPos + Quaternion.Euler(finalEuler) *
                            Graph.ElementwiseMultiply(center, finalScale), finalEuler,
                            Graph.ElementwiseMultiply(size, finalScale)));
                    }
                    else
                        res.Add(Mesh.GetMesh(box, finalPos, finalEuler, finalScale));
                }
                else if (c is SphereCollider sp)
                {
                    res.Add(Mesh.GetMesh(sp, finalPos, finalEuler, finalScale));
                }
            }
            return res;
        }

        /// <summary>将最终本体的局部 -Z 边界贴合组合模型的后边界；Trigger 共用本体偏移。</summary>
        public Vector3 GetObjectBackColliderOffset(GameObject root, Rect? firstPartBounds = null,
            float boundsColliderScale = 1f, Vector3? rootScale = null)
        {
            if (root.transform.childCount == 0)
                return Vector3.zero;
            var scale = rootScale ?? Vector3.one;
            float zSign = scale.z < 0f ? -1f : 1f;
            var bodies = GetCollidersMesh(root, Vector3.zero, Vector3.zero, scale,
                CollideType.CollideOnly, firstPartBounds, boundsColliderScale);
            if (bodies.Count == 0)
                bodies = GetCollidersMesh(root, Vector3.zero, Vector3.zero, scale,
                    CollideType.TriggerOnly, firstPartBounds, boundsColliderScale);
            if (bodies.Count == 0)
                return Vector3.zero;

            float colliderBack = float.PositiveInfinity;
            foreach (var body in bodies)
            {
                if (body.type == MeshType.Sphere)
                {
                    float radius = (body.positions[(int)Graph.SphereSixPoint.Right]
                        - body.positions[(int)Graph.SphereSixPoint.Left]).magnitude * .5f;
                    colliderBack = Mathf.Min(colliderBack, body.center.z * zSign - radius);
                }
                else
                    foreach (var point in body.positions)
                        colliderBack = Mathf.Min(colliderBack, point.z * zSign);
            }
            float modelBack = float.PositiveInfinity;
            foreach (Transform part in root.transform)
            {
                // Combined parts use the same unit-cube footprint as the visual
                // ownership/WangTile index, independently of the Collider scale.
                var half = part.localScale * .5f;
                var rotation = part.localRotation;
                float depth = Mathf.Abs((rotation * new Vector3(half.x, 0, 0)).z)
                    + Mathf.Abs((rotation * new Vector3(0, half.y, 0)).z)
                    + Mathf.Abs((rotation * new Vector3(0, 0, half.z)).z);
                modelBack = Mathf.Min(modelBack, part.localPosition.z - depth);
            }
            // Translate by the difference of the two rear EDGES, not by the
            // difference of their centers. For a unit Box with scale s this
            // keeps rear=-0.5 and center=-0.5+s/2, rather than center=-0.5.
            float rearBoundaryDelta = modelBack * Mathf.Abs(scale.z) - colliderBack;
            return Vector3.forward * (zSign * rearBoundaryDelta);
        }
        public List<MapUnit> CaptureCast(Vector3 from, Vector3 to, float radius)
        {
            var res = new List<MapUnit>();

            HashSet<int> exist = new HashSet<int>();

            var fromMesh = Mesh.GetMesh(from, radius, Vector3.zero, Vector3.one);
            var toMesh = Mesh.GetMesh(to, radius, Vector3.zero, Vector3.one);
            // The cast sweeps across the whole segment. Sampling only the two
            // endpoint spheres misses intermediate Tile and Object colliders.
            var sweepPoints = new Vector3[fromMesh.positions.Length + toMesh.positions.Length];
            fromMesh.positions.CopyTo(sweepPoints, 0);
            toMesh.positions.CopyTo(sweepPoints, fromMesh.positions.Length);
            var lst = Graph.GetRoughOverlapIntPos(sweepPoints);


            var checkedColumns = new HashSet<(int, int)>();
            foreach (var pos in lst)
            {
                var mapPos = RealPos2MapPosInt(pos);
                if (!checkedColumns.Add((mapPos.x, mapPos.z)) ||
                    !_super.data.mapXZ2Y.TryGetValue((mapPos.x, mapPos.z), out var yLevels))
                    continue;

                // A Tile collider can extend into a different logical height layer.
                // Check every real Tile in this column and let the mesh sweep decide.
                foreach (var y in yLevels)
                {
                    if (!_super.data.maps.TryGetValue((mapPos.x, y, mapPos.z), out var tileData))
                        continue;
                    var tile = tileData.unit;
                    var unitLst = new List<MapUnit>() { tile };
                    if (_super.updateCtrl.objectTileDic.TryGet(tile, out var objects))
                        unitLst.AddRange(objects);
                    if (_super.updateCtrl.characterOverlapTileDic.TryGet(tile, out var characters))
                        unitLst.AddRange(characters);
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
            }
            Debug.DrawLine(from, to, Color.green);
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
            TileUnit owner = GetPlacementTile(oData.pos);
            if (owner != null && added.Add(owner))
                result.Add(owner);

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

        public bool TryGetObjectNavigationRange(MeshInfo mesh, out Vector3Int min, out Vector3Int max)
        {
            min = max = Vector3Int.zero;
            if (!TryGetObjectProbeBounds(mesh, out var bounds))
                return false;

            Vector3 low = bounds.min, high = bounds.max;
            low = RealPos2MapPos(low);
            high = RealPos2MapPos(high);
            // Include central 0.8-square probes even when a cell is narrower.
            // Central/edge physical-body clipping narrows this conservative range.
            const float epsilon = 0.0001f;
            var cellSize = _super.data.mainData.mapUnitSize;
            float probeX = Mathf.Max(.5f, Analysis.NavigationController.ObjectNavigationHalfWidth / Mathf.Max(Mathf.Abs(cellSize.x), epsilon));
            float probeZ = Mathf.Max(.5f, Analysis.NavigationController.ObjectNavigationHalfWidth / Mathf.Max(Mathf.Abs(cellSize.z), epsilon));
            min = new Vector3Int(Mathf.CeilToInt(low.x - probeX - epsilon),
                Mathf.CeilToInt(low.y - 0.5f - epsilon), Mathf.CeilToInt(low.z - probeZ - epsilon));
            max = new Vector3Int(Mathf.FloorToInt(high.x + probeX + epsilon),
                Mathf.FloorToInt(high.y + 0.5f + epsilon), Mathf.FloorToInt(high.z + probeZ + epsilon));
            return true;
        }

        public List<TileUnit> GetObjectNavigationOverlap(ObjectUnitForm.Data oData)
        {
            var result = new List<TileUnit>();
            var added = new HashSet<TileUnit>();
            // Index physical bodies even before Play enables product.collision.
            foreach (var mesh in oData.unit.GetMeshes(CollideType.CollideOnly))
            {
                if (!TryGetObjectNavigationRange(mesh, out var min, out var max))
                    continue;
                for (int x = min.x; x <= max.x; x++)
                for (int y = min.y; y <= max.y; y++)
                for (int z = min.z; z <= max.z; z++)
                {
                    var tile = GetTileData(x, y, z)?.unit;
                    if (tile != null && added.Add(tile))
                        result.Add(tile);
                }
            }
            return result;
        }

        public List<TileUnit> GetObjectPassTypeOverlap(ObjectUnitForm.Data oData)
        {
            var result = new List<TileUnit>();
            var bodies = oData.unit.GetMeshes(CollideType.CollideOnly);
            if (bodies.Count > 0)
            {
                var added = new HashSet<TileUnit>();
                foreach (var body in bodies)
                {
                    if (!TryGetObjectProbeBounds(body, out var bodyBounds))
                        continue;
                    // Collider transforms already contain MapModel.colliderScale.
                    // Scan each body's local range, never the visual footprint or Trigger.
                    Vector3 low = RealPos2MapPos(bodyBounds.min - Vector3.up * ObjectTileProbeHeight);
                    Vector3 high = RealPos2MapPos(bodyBounds.max);
                    const float epsilon = 0.0001f;
                    for (int x = Mathf.CeilToInt(low.x - 0.5f - epsilon); x <= Mathf.FloorToInt(high.x + 0.5f + epsilon); x++)
                    for (int y = Mathf.CeilToInt(low.y - 0.5f - epsilon); y <= Mathf.FloorToInt(high.y + 0.5f + epsilon); y++)
                    for (int z = Mathf.CeilToInt(low.z - 0.5f - epsilon); z <= Mathf.FloorToInt(high.z + 0.5f + epsilon); z++)
                    {
                        var tile = GetTileData(x, y, z)?.unit;
                        if (tile != null && !added.Contains(tile) && DoesTileProbeIntersectMesh(body, tile.data.pos))
                        {
                            added.Add(tile);
                            result.Add(tile);
                        }
                    }
                }
                return result;
            }

            // Preserve the old body fallback for custom prefabs without physical Colliders.
            TryGetObjectBounds(oData, out Bounds bounds, true);
            GameObject root = oData.unit.prefab;
            Matrix4x4 rootToRuntime = Matrix4x4.TRS(oData.pos, Quaternion.Euler(oData.euler), oData.scale);
            // Only cells whose upward probe can reach this Object are candidates.
            // This is separate from the visual/owner index: a probe may reach an
            // Object above its cell even when that Object is not visually in it.
            Vector3 min = RealPos2MapPos(bounds.min - Vector3.up * ObjectTileProbeHeight);
            Vector3 max = RealPos2MapPos(bounds.max);
            int minX = Mathf.FloorToInt(min.x - 0.5f) + 1;
            int maxX = Mathf.CeilToInt(max.x + 0.5f) - 1;
            int minY = Mathf.FloorToInt(min.y - 0.5f) + 1;
            int maxY = Mathf.CeilToInt(max.y + 0.5f) - 1;
            int minZ = Mathf.FloorToInt(min.z - 0.5f) + 1;
            int maxZ = Mathf.CeilToInt(max.z + 0.5f) - 1;
            for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
            for (int z = minZ; z <= maxZ; z++)
            {
                var tile = GetTileData(x, y, z)?.unit;
                if (tile != null && IsTileCenterCoveredByObject(tile, oData, bounds, root, rootToRuntime))
                    result.Add(tile);
            }
            return result;
        }

        public bool IsTileCenterCoveredByObject(TileUnit tile, ObjectUnitForm.Data oData)
        {
            if (tile == null || oData == null)
                return false;

            var bodies = oData.unit.GetMeshes(CollideType.CollideOnly);
            if (bodies.Count > 0)
            {
                foreach (var body in bodies)
                    if (DoesTileProbeIntersectMesh(body, tile.data.pos))
                        return true;
                return false;
            }

            TryGetObjectBounds(oData, out Bounds bounds, true);
            GameObject root = oData.unit.prefab;
            Matrix4x4 rootToRuntime = Matrix4x4.TRS(oData.pos, Quaternion.Euler(oData.euler), oData.scale);
            return IsTileCenterCoveredByObject(tile, oData, bounds, root, rootToRuntime);
        }

        private static bool IsTileCenterCoveredByObject(TileUnit tile, ObjectUnitForm.Data oData,
            Bounds bounds, GameObject root, Matrix4x4 rootToRuntime)
        {
            Vector3 center = tile.data.pos;
            const float epsilon = 0.0001f;
            // Tile中心向上0~1世界单位的线段，不能只检测中心点或水平投影。
            if (center.x < bounds.min.x - epsilon || center.x > bounds.max.x + epsilon
                || center.y + ObjectTileProbeHeight < bounds.min.y - epsilon
                || center.y > bounds.max.y + epsilon
                || center.z < bounds.min.z - epsilon || center.z > bounds.max.z + epsilon)
                return false;

            if (root != null)
            {
                if (root.GetComponent<ObjectInstance>() != null && root.transform.childCount > 0)
                {
                    for (int i = 0; i < root.transform.childCount; i++)
                    {
                        Transform child = root.transform.GetChild(i);
                        Vector3 visualScale = new Vector3(
                            Mathf.Abs(child.localScale.x), Mathf.Abs(child.localScale.y), Mathf.Abs(child.localScale.z));
                        Matrix4x4 visualToRuntime = rootToRuntime
                            * Matrix4x4.TRS(child.localPosition, child.localRotation, visualScale);
                        if (DoesTileProbeIntersectBox(visualToRuntime,
                            new Bounds(Vector3.zero, Vector3.one), center))
                            return true;
                    }
                    return false;
                }

                Matrix4x4 prefabWorldToRoot = root.transform.worldToLocalMatrix;
                bool hasRenderer = false;
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    if (!renderer.enabled
                        || renderer.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly)
                        continue;
                    hasRenderer = true;
                    Matrix4x4 rendererToRuntime = rootToRuntime
                        * prefabWorldToRoot * renderer.transform.localToWorldMatrix;
                    if (DoesTileProbeIntersectBox(rendererToRuntime, renderer.localBounds, center))
                        return true;
                }
                if (hasRenderer)
                    return false;
            }

            Quaternion rotation = Quaternion.Euler(oData.euler);
            Vector3 fallbackScale = new Vector3(
                Mathf.Abs(oData.scale.x), Mathf.Abs(oData.scale.y), Mathf.Abs(oData.scale.z));
            Vector3 fallbackCenter = oData.pos + rotation * new Vector3(0f, fallbackScale.y * 0.5f, 0f);
            return DoesTileProbeIntersectBox(Matrix4x4.TRS(fallbackCenter, rotation, fallbackScale),
                new Bounds(Vector3.zero, Vector3.one), center);
        }

        private static float GetProbeSphereRadius(MeshInfo body)
        {
            // Match Graph.SphereIntersectCube's physical sphere radius.
            return (body.positions[(int)Graph.SphereSixPoint.Right]
                - body.positions[(int)Graph.SphereSixPoint.Left]).magnitude * 0.5f;
        }

        private static bool TryGetObjectProbeBounds(MeshInfo body, out Bounds bounds)
        {
            bounds = default;
            if (body.positions == null || body.positions.Length == 0)
                return false;
            if (body.type == MeshType.Sphere && body.positions.Length >= 6)
            {
                bounds = new Bounds(body.center, Vector3.one * (GetProbeSphereRadius(body) * 2f));
                return true;
            }
            if (body.type != MeshType.Cube || body.positions.Length < 8)
                return false;
            bounds = new Bounds(body.positions[0], Vector3.zero);
            foreach (var vertex in body.positions)
                bounds.Encapsulate(vertex);
            return true;
        }

        private static bool DoesTileProbeIntersectMesh(MeshInfo body, Vector3 point)
        {
            if (body.positions == null)
                return false;
            if (body.type == MeshType.Sphere && body.positions.Length >= 6)
            {
                Vector3 closest = new Vector3(point.x,
                    Mathf.Clamp(body.center.y, point.y, point.y + ObjectTileProbeHeight), point.z);
                float radius = GetProbeSphereRadius(body);
                if (radius <= 0f)
                    return false;
                radius += 0.0001f;
                return (closest - body.center).sqrMagnitude <= radius * radius;
            }
            if (body.type != MeshType.Cube || body.positions.Length < 8)
                return false;
            var points = body.positions;
            // colliderScale=0 has no horizontal body footprint. Thin/flat Y
            // bodies remain valid so zero-height bridge planes still work.
            if ((points[1] - points[0]).sqrMagnitude == 0f
                || (points[4] - points[0]).sqrMagnitude == 0f)
                return false;
            return DoesTileProbeIntersectBox(body.center, (points[1] - points[0]) * 0.5f,
                (points[2] - points[0]) * 0.5f, (points[4] - points[0]) * 0.5f, point);
        }

        private static bool DoesTileProbeIntersectBox(Matrix4x4 localToWorld, Bounds localBounds, Vector3 point)
        {
            Vector3 boxCenter = localToWorld.MultiplyPoint3x4(localBounds.center);
            Vector3 extents = localBounds.extents;
            Vector3 edgeX = localToWorld.MultiplyVector(new Vector3(extents.x, 0, 0));
            Vector3 edgeY = localToWorld.MultiplyVector(new Vector3(0, extents.y, 0));
            Vector3 edgeZ = localToWorld.MultiplyVector(new Vector3(0, 0, extents.z));
            return DoesTileProbeIntersectBox(boxCenter, edgeX, edgeY, edgeZ, point);
        }

        private static bool DoesTileProbeIntersectBox(Vector3 boxCenter, Vector3 edgeX,
            Vector3 edgeY, Vector3 edgeZ, Vector3 point)
        {
            Vector3 offset = point + Vector3.up * (ObjectTileProbeHeight * 0.5f) - boxCenter;
            // Segment/box SAT: box face normals and edge/segment cross products.
            // Cardinal axes additionally cover degenerate flat/zero-size boxes.
            // This also handles tilted/sheared boxes without inverse matrices or arrays.
            return IsInTileProbeBoxAxis(offset, edgeX, edgeY, edgeZ, Vector3.right)
                && IsInTileProbeBoxAxis(offset, edgeX, edgeY, edgeZ, Vector3.up)
                && IsInTileProbeBoxAxis(offset, edgeX, edgeY, edgeZ, Vector3.forward)
                && IsInTileProbeBoxAxis(offset, edgeX, edgeY, edgeZ, Vector3.Cross(edgeX, edgeY))
                && IsInTileProbeBoxAxis(offset, edgeX, edgeY, edgeZ, Vector3.Cross(edgeY, edgeZ))
                && IsInTileProbeBoxAxis(offset, edgeX, edgeY, edgeZ, Vector3.Cross(edgeZ, edgeX))
                && IsInTileProbeBoxAxis(offset, edgeX, edgeY, edgeZ, Vector3.Cross(edgeX, Vector3.up))
                && IsInTileProbeBoxAxis(offset, edgeX, edgeY, edgeZ, Vector3.Cross(edgeY, Vector3.up))
                && IsInTileProbeBoxAxis(offset, edgeX, edgeY, edgeZ, Vector3.Cross(edgeZ, Vector3.up));
        }

        private static bool IsInTileProbeBoxAxis(Vector3 offset, Vector3 x, Vector3 y, Vector3 z, Vector3 axis)
        {
            float radius = Mathf.Abs(Vector3.Dot(x, axis)) + Mathf.Abs(Vector3.Dot(y, axis))
                + Mathf.Abs(Vector3.Dot(z, axis)) + Mathf.Abs(axis.y) * (ObjectTileProbeHeight * 0.5f);
            return Mathf.Abs(Vector3.Dot(offset, axis)) <= radius + 0.0001f * axis.magnitude;
        }

        public bool TryGetVisionBounds(ObjectUnitForm.Data oData, out Bounds visionBounds)
        {
            return TryGetObjectBounds(oData, out visionBounds, false);
        }

        private bool TryGetObjectBounds(ObjectUnitForm.Data oData, out Bounds visionBounds, bool useBodyCenter)
        {
            return TryGetObjectBounds(oData, out visionBounds, useBodyCenter, out _);
        }

        // The fixed 45-degree side view projects a world point to ground Z as
        // z + y - groundY. Accumulate actual corners, not unrelated AABB extrema.
        public Vector2 GetVisionProjectionZRange(ObjectUnitForm.Data oData)
        {
            TryGetObjectBounds(oData, out _, false, out Vector2 projectedZ);
            return projectedZ;
        }

        private static void EncapsulateObjectCorner(
            Vector3 corner, ref Bounds bounds, ref Vector2 projectedZ, ref bool hasBounds)
        {
            float projection = corner.z + corner.y;
            if (hasBounds)
            {
                bounds.Encapsulate(corner);
                projectedZ.x = Mathf.Min(projectedZ.x, projection);
                projectedZ.y = Mathf.Max(projectedZ.y, projection);
            }
            else
            {
                bounds = new Bounds(corner, Vector3.zero);
                projectedZ = new Vector2(projection, projection);
                hasBounds = true;
            }
        }

        private bool TryGetObjectBounds(
            ObjectUnitForm.Data oData, out Bounds visionBounds, bool useBodyCenter, out Vector2 projectedZ)
        {
            visionBounds = default;
            projectedZ = default;
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
                        // Pass-type probes use the actual centered primitive/model body.
                        // The display footprint keeps its existing base/height convention.
                        Vector3 visualCenter = useBodyCenter ? child.localPosition
                            : visualBase + Vector3.up * (visualScale.y * 0.5f);
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
                            EncapsulateObjectCorner(worldCorner, ref visionBounds, ref projectedZ, ref hasBounds);
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
                        EncapsulateObjectCorner(worldCorner, ref visionBounds, ref projectedZ, ref hasBounds);
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
            for (int x = 0; x < 2; x++)
            for (int y = 0; y < 2; y++)
            for (int z = 0; z < 2; z++)
            {
                Vector3 corner = center + rotation * Vector3.Scale(scale, new Vector3(
                    x == 0 ? -.5f : .5f, y == 0 ? -.5f : .5f, z == 0 ? -.5f : .5f));
                EncapsulateObjectCorner(corner, ref visionBounds, ref projectedZ, ref hasBounds);
            }
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
