//#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;
using Z_DesignStyle;
using Z_Map.Form;
using Z_Math;

using Mesh = Z_Mesh.Mesh;

namespace Z_Map.Analysis
{
    public enum Dir
    {
        Right,
        Left,
        Forward,
        Back
    }
    public class NavUnit
    {
        public bool isNull;
        public bool objectBlocked;
        // Right/Left/Forward/Back bits; separate from central standability.
        public int objectBlockedDirections;
        public Vector3Int pos;
        public Vector3 realPos;
        public List<NavUnit> links;
        public float[] dirMaxY;
        public float[] dirGroundY;
        public HashSet<int> passTypes;
    }
    public interface NaviComponent
    {
        public Vector3 GetNextDir(Vector3 cur, Vector3 tar, int maxStep, float agentRadius,
            IReadOnlyCollection<int> passTypes, NavigationEndpointState endpointState = null);
    }

    /// <summary>每个角色独立保存端点直线段，不能放在共享 BFS 中。</summary>
    public sealed class NavigationEndpointState
    {
        internal NavigationController controller;
        internal Vector3 destination;
        internal float radius;
        internal readonly HashSet<int> passTypes = new HashSet<int>();
        internal NavUnit escapeAnchor;
        internal bool finalApproach;
        public Vector3 moveTarget { get; internal set; }
        public bool directEndpoint { get; internal set; }

        public void Reset()
        {
            controller = null;
            escapeAnchor = null;
            finalApproach = false;
            directEndpoint = false;
            passTypes.Clear();
        }

        internal void Prepare(NavigationController current, Vector3 target, float agentRadius,
            IReadOnlyCollection<int> allowedTypes)
        {
            bool changedCell = controller == current && !destination.Equals(target)
                && current.RealPos2MapPosInt(destination) != current.RealPos2MapPosInt(target);
            if (controller != current || changedCell || Mathf.Abs(radius - agentRadius) > 0.0001f
                || (allowedTypes == null ? passTypes.Count != 0 : !passTypes.SetEquals(allowedTypes)))
            {
                Reset();
                controller = current;
                radius = agentRadius;
                if (allowedTypes != null)
                    passTypes.UnionWith(allowedTypes);
            }
            // 索敌目标在同一格内移动时继续当前末段，但始终朝最新坐标移动。
            destination = target;
            directEndpoint = false;
        }
    }
    public class NavigationController : Z_Controller<MapManager>
    {
        private const string MapGroundPrefabName = "MapPrefab$mapground";
        private const float PassableObstacleHeight = 0.2f;
        private const float ObjectBlockHeight = 0.3f;
        public const float ObjectNavigationHalfWidth = 0.4f;
        private const float GeometryEpsilon = 0.0001f;
        private readonly Vector3[] objectClipA = new Vector3[16];
        private readonly Vector3[] objectClipB = new Vector3[16];
        private static readonly int[,] objectBoxFaces =
        {
            { 0, 1, 3, 2 }, { 4, 6, 7, 5 }, { 0, 2, 6, 4 },
            { 1, 5, 7, 3 }, { 0, 4, 5, 1 }, { 2, 3, 7, 6 }
        };

        private sealed class MapGroundCoverageCache
        {
            public string prefabName;
            public GameObject prefab;
            public Vector3Int mapPos;
            public Vector3 pos;
            public Vector3 euler;
            public Vector3 scale;
            public Vector3 cellSize;
            public readonly List<(int, int, int)> coveredNavUnits = new List<(int, int, int)>();
            public readonly HashSet<(int, int, int)> candidateNavUnits = new HashSet<(int, int, int)>();

            public bool Matches(TileUnitForm.Data map, GameObject currentPrefab, Vector3 currentCellSize)
            {
                return prefabName == map.prefabName
                    && prefab == currentPrefab
                    && mapPos == map.mapPos
                    && pos == map.pos
                    && euler == map.euler
                    && scale == map.scale
                    && cellSize == currentCellSize;
            }
        }

        public NavigationController(MapManager super) : base(super)
        { }
        NaviComponent bfs;
        public Dictionary<(int, int, int), NavUnit> navUnits;
        public float step;
        private readonly Dictionary<int, MapGroundCoverageCache> mapGroundCoverageCaches =
            new Dictionary<int, MapGroundCoverageCache>();
        private readonly Dictionary<(int, int, int), HashSet<int>> mapGroundCandidateSources =
            new Dictionary<(int, int, int), HashSet<int>>();
        private readonly HashSet<(int, int, int)> mapGroundBlocked = new HashSet<(int, int, int)>();
        private readonly Dictionary<(int, int, int), NavUnit> objectRefreshUnits = new Dictionary<(int, int, int), NavUnit>();
        private readonly HashSet<ObjectUnit> objectRefreshCandidates = new HashSet<ObjectUnit>();
        private readonly HashSet<NavUnit> objectRefreshLinkSources = new HashSet<NavUnit>();
        private readonly HashSet<TileUnit> objectChangesDuringRebuild = new HashSet<TileUnit>();
        private int activeFullRebuilds;
        private bool navCellLayoutInitialized;
        private int navCellLayoutCount;
        private int navCellLayoutHash;

        public void Build()
        {
            step = _super.data.mainData.mapUnitSize.y * 1 / 3;
            navUnits = new Dictionary<(int, int, int), NavUnit>(_super.data.maps.Count);
            mapGroundCoverageCaches.Clear();
            mapGroundCandidateSources.Clear();
            navCellLayoutInitialized = false;
            RebuildNow();
            bfs = new Bfs(this);
        }
        Vector3Int[] tryDir = new Vector3Int[] { Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back };
        Vector2[] offset = new Vector2[] { Vector2.right * 0.25f, Vector2.left * 0.25f, Vector2.up * 0.25f, Vector2.down * 0.25f };
        List<TileUnitForm.Data> curUpdateTileList = new List<TileUnitForm.Data>();
        List<ObjectUnitForm.Data> curUpdateObjList = new List<ObjectUnitForm.Data>();
        int curUpdateCount = 0;
        public void UpdateMap(int step)
        {
            _super.StartCoroutine(UpdateInternal(step));
        }
        public void RebuildNow()
        {
            var update = UpdateInternal(int.MaxValue);
            while (update.MoveNext()) { }
        }
        IEnumerator UpdateInternal(int step)
        {
            activeFullRebuilds++;
            try
            {
                var update = RebuildInternal(step);
                while (update.MoveNext())
                    yield return update.Current;
            }
            finally
            {
                activeFullRebuilds--;
                if (activeFullRebuilds == 0 && objectChangesDuringRebuild.Count > 0)
                {
                    RefreshObjectTiles(objectChangesDuringRebuild);
                    objectChangesDuringRebuild.Clear();
                }
            }
        }

        IEnumerator RebuildInternal(int step)
        {
            int times = step == int.MaxValue ? 0 : step;
            int currentNavCellLayoutCount = 0;
            int currentNavCellLayoutHash = 17;
            curUpdateCount = 0;
            //build single unit
            curUpdateTileList.Clear();
            curUpdateTileList.AddRange(_super.data.maps.Values);
            for (; curUpdateCount < curUpdateTileList.Count; curUpdateCount++)
            {
                times++;
                if (times >= step)
                {
                    times = 0;
                    yield return null;
                }
                if (!_super.enable)
                {
                    yield break;
                }
                var map = curUpdateTileList[curUpdateCount];
                if (!TileUnitForm.DataByUid.ContainsKey(map.uid))
                    continue;
                currentNavCellLayoutCount++;
                unchecked
                {
                    currentNavCellLayoutHash = currentNavCellLayoutHash * 31 + map.uid;
                    currentNavCellLayoutHash = currentNavCellLayoutHash * 31 + map.mapPos.GetHashCode();
                    currentNavCellLayoutHash = currentNavCellLayoutHash * 31 + map.pos.GetHashCode();
                }
                (int, int, int) pos = (map.mapPos.x, map.mapPos.y, map.mapPos.z);
                if (!navUnits.ContainsKey(pos))
                {
                    navUnits[pos] = new NavUnit
                    {
                        links = new List<NavUnit>()
                    };
                }

                var navUnit = navUnits[pos];
                navUnit.realPos = map.pos;
                navUnit.pos = map.mapPos;
                navUnit.isNull = map.scale == Vector3.zero;
                navUnit.objectBlocked = false;
                navUnit.objectBlockedDirections = 0;
                navUnit.passTypes = new HashSet<int>(map.unit.passTypes);
                navUnit.dirMaxY = new float[4]
                {
                    map.unit.GetYByPoint(offset[0]),
                    map.unit.GetYByPoint(offset[1]),
                    map.unit.GetYByPoint(offset[2]),
                    map.unit.GetYByPoint(offset[3])
                };
                navUnit.dirGroundY = (float[])navUnit.dirMaxY.Clone();
            }

            // Adding/removing a NavUnit, or moving its cell center, can change
            // which cells an otherwise unchanged mapground volume covers.
            if (!navCellLayoutInitialized
                || navCellLayoutCount != currentNavCellLayoutCount
                || navCellLayoutHash != currentNavCellLayoutHash)
            {
                mapGroundCoverageCaches.Clear();
                mapGroundCandidateSources.Clear();
                navCellLayoutInitialized = true;
                navCellLayoutCount = currentNavCellLayoutCount;
                navCellLayoutHash = currentNavCellLayoutHash;
            }
            //先扫描障碍物，记录blocked位置
            curUpdateObjList.Clear();
            curUpdateObjList.AddRange(ObjectUnitForm.DataByUid.Values);
            curUpdateCount = 0;
            var blocked = new HashSet<(int, int, int)>();

            // mapground 是实心地块。它的 Collider 可能向下覆盖一个或多个逻辑层；
            // 顶面仍可行走，但被实体体积占据的下层导航单元必须标记为阻挡。
            for (; curUpdateCount < curUpdateTileList.Count; curUpdateCount++)
            {
                times++;
                if (times >= step)
                {
                    times = 0;
                    yield return null;
                }
                if (!_super.enable)
                {
                    yield break;
                }

                var map = curUpdateTileList[curUpdateCount];
                if (!TileUnitForm.DataByUid.ContainsKey(map.uid))
                    continue;

                MarkMapGroundCoveredNavUnits(map, blocked);
            }

            curUpdateCount = 0;
            for (; curUpdateCount < curUpdateObjList.Count; curUpdateCount++)
            {
                var obs = curUpdateObjList[curUpdateCount];
                times++;
                if (times >= step)
                {
                    times = 0;
                    yield return null;
                }
                if (!_super.enable)
                {
                    yield break;
                }
                if (!ObjectUnitForm.DataByUid.ContainsKey(obs.uid))
                    continue;
                ApplyObjectObstacle(obs, navUnits);
            }

            /*         curUpdateCount = 0;
                     for (; curUpdateCount < curUpdateTileList.Count; curUpdateCount++)
                     {
                         times++;
                         if (times >= step)
                         {
                             times = 0;
                             yield return null;
                         }
                         if (!_super.enable)
                         {
                             yield break;
                         }
                         var map = curUpdateTileList[curUpdateCount];
                         if (!TileUnitForm.DataByUid.ContainsKey(map.uid))
                             continue;
                         (int, int, int) pos = (map.mapPos.x, map.mapPos.y, map.mapPos.z);
                         if (navUnits.TryGetValue(pos, out var unit))
                         {
                             float max = float.MinValue;
                             float min = float.MaxValue;
                             for (int dir = 0; dir < offset.Length; dir++)
                             {
                                 max = Mathf.Max(unit.dirMaxY[dir], max);
                                 min = Mathf.Max(unit.dirMaxY[dir], min);
                             }
                             if (max - min > this.step)
                             {
                                 blocked.Add(pos);
                             }
                         }


                     }*/
            //构建links，跳过blocked的tile
            curUpdateCount = 0;
            for (; curUpdateCount < curUpdateTileList.Count; curUpdateCount++)
            {
                times++;
                if (times >= step)
                {
                    times = 0;
                    yield return null;
                }
                if (!_super.enable)
                {
                    yield break;
                }
                var map = curUpdateTileList[curUpdateCount];
                if (!TileUnitForm.DataByUid.ContainsKey(map.uid))
                    continue;
                (int, int, int) pos = (map.mapPos.x, map.mapPos.y, map.mapPos.z);

                RebuildLinks(navUnits[pos], blocked);
            }
            mapGroundBlocked.Clear();
            mapGroundBlocked.UnionWith(blocked);
        }

        public void RefreshObjectTiles(IEnumerable<TileUnit> tiles)
        {
            if (navUnits == null || !_super.enable)
                return;

            var affected = objectRefreshUnits;
            var objects = objectRefreshCandidates;
            affected.Clear();
            objects.Clear();
            foreach (var tile in tiles)
            {
                if (activeFullRebuilds > 0)
                    objectChangesDuringRebuild.Add(tile);
                var pos = tile.data.mapPos;
                var key = (pos.x, pos.y, pos.z);
                if (!_super.data.maps.TryGetValue(key, out var current)
                    || current.unit != tile
                    || !navUnits.TryGetValue(key, out var navUnit))
                    continue;

                affected[key] = navUnit;
                navUnit.objectBlocked = false;
                navUnit.objectBlockedDirections = 0;
                navUnit.passTypes.Clear();
                foreach (int passType in tile.passTypes)
                    navUnit.passTypes.Add(passType);
                Array.Copy(navUnit.dirGroundY, navUnit.dirMaxY, navUnit.dirGroundY.Length);

                if (_super.updateCtrl.objectNavigationTileDic.TryGet(tile, out var overlapping))
                    foreach (var obj in overlapping)
                        objects.Add(obj);
            }

            if (affected.Count == 0)
                return;

            foreach (var obj in objects)
                if (ObjectUnitForm.DataByUid.ContainsKey(obj.data.uid))
                    ApplyObjectObstacle(obj.data, affected);

            // A changed obstacle height affects links entering the changed cell too.
            var linkSources = objectRefreshLinkSources;
            linkSources.Clear();
            foreach (var navUnit in affected.Values)
                linkSources.Add(navUnit);
            foreach (var navUnit in affected.Values)
                for (int dy = -1; dy <= 1; dy++)
                    for (int dir = 0; dir < tryDir.Length; dir++)
                    {
                        var origin = navUnit.pos - tryDir[dir] - new Vector3Int(0, dy, 0);
                        if (navUnits.TryGetValue((origin.x, origin.y, origin.z), out var source))
                            linkSources.Add(source);
                    }

            foreach (var navUnit in linkSources)
                RebuildLinks(navUnit, mapGroundBlocked);
            affected.Clear();
            objects.Clear();
            linkSources.Clear();
        }

        // Edit/undo only resamples changed terrain and the solid volumes whose
        // conservative footprints contain it, including previously empty cells.
        public void RefreshTerrainTiles(IEnumerable<Vector3Int> positions)
        {
            if (navUnits == null || !_super.enable)
                return;
            var keys = new HashSet<(int, int, int)>();
            var sources = new HashSet<int>();
            foreach (var pos in positions)
            {
                var key = (pos.x, pos.y, pos.z);
                keys.Add(key);
                if (mapGroundCandidateSources.TryGetValue(key, out var candidates))
                    sources.UnionWith(candidates);
                if (!_super.data.maps.TryGetValue(key, out var tile))
                {
                    navUnits.Remove(key);
                    continue;
                }
                sources.Add(tile.uid);
                if (!navUnits.TryGetValue(key, out var unit))
                {
                    unit = new NavUnit { links = new List<NavUnit>(), passTypes = new HashSet<int>(),
                        dirGroundY = new float[4], dirMaxY = new float[4] };
                    navUnits.Add(key, unit);
                }
                unit.pos = tile.mapPos;
                unit.realPos = tile.pos;
                unit.isNull = tile.scale == Vector3.zero;
                for (int i = 0; i < offset.Length; i++)
                    unit.dirGroundY[i] = unit.dirMaxY[i] = tile.unit.GetYByPoint(offset[i]);
            }
            if (keys.Count == 0)
                return;
            foreach (int uid in sources)
            {
                if (mapGroundCoverageCaches.TryGetValue(uid, out var previous))
                {
                    keys.UnionWith(previous.coveredNavUnits);
                    RemoveGroundCoverage(uid, previous);
                }
                if (TileUnitForm.DataByUid.TryGetValue(uid, out var tile)
                    && tile.prefabName == MapGroundPrefabName && tile.scale != Vector3.zero)
                {
                    var coverage = BuildMapGroundCoverage(tile, tile.unit.prefab, _super.data.mainData.mapUnitSize);
                    mapGroundCoverageCaches[uid] = coverage;
                    keys.UnionWith(coverage.coveredNavUnits);
                    RegisterGroundCandidates(uid, coverage);
                }
            }
            var tiles = new List<TileUnit>();
            foreach (var key in keys)
            {
                bool blocked = false;
                if (mapGroundCandidateSources.TryGetValue(key, out var candidates))
                    foreach (int uid in candidates)
                        if (mapGroundCoverageCaches.TryGetValue(uid, out var source)
                            && source.coveredNavUnits.Contains(key))
                        { blocked = true; break; }
                if (blocked) mapGroundBlocked.Add(key);
                else mapGroundBlocked.Remove(key);
                if (_super.data.maps.TryGetValue(key, out var tile))
                    tiles.Add(tile.unit);
            }
            RefreshObjectTiles(tiles);
            // Deleted cells have no Tile to pass to RefreshObjectTiles, but their
            // surviving neighbours must immediately drop incoming links.
            foreach (var key in keys)
                for (int dy = -1; dy <= 1; dy++)
                    foreach (var dir in tryDir)
                        if (navUnits.TryGetValue((key.Item1 + dir.x, key.Item2 + dy, key.Item3 + dir.z), out var source))
                            RebuildLinks(source, mapGroundBlocked);
            navCellLayoutInitialized = false;
        }

        private void RegisterGroundCandidates(int uid, MapGroundCoverageCache coverage)
        {
            foreach (var key in coverage.candidateNavUnits)
            {
                if (!mapGroundCandidateSources.TryGetValue(key, out var sources))
                    mapGroundCandidateSources.Add(key, sources = new HashSet<int>());
                sources.Add(uid);
            }
        }

        private void RemoveGroundCoverage(int uid, MapGroundCoverageCache coverage)
        {
            foreach (var key in coverage.candidateNavUnits)
                if (mapGroundCandidateSources.TryGetValue(key, out var sources))
                {
                    sources.Remove(uid);
                    if (sources.Count == 0) mapGroundCandidateSources.Remove(key);
                }
            mapGroundCoverageCaches.Remove(uid);
        }

        private void ApplyObjectObstacle(ObjectUnitForm.Data obs, Dictionary<(int, int, int), NavUnit> targetUnits)
        {
            if (obs == null || !obs.isObstacle)
                return;

            // Share the physical collision geometry, including nested offsets and
            // rotations. Trigger padding is not a walkable surface or obstacle.
            foreach (var mesh in obs.unit.GetMeshes(CollideType.CollideOnly))
            {
                if (!_super.utilCtrl.TryGetObjectNavigationRange(mesh, out var min, out var max))
                    continue;
                for (int x = min.x; x <= max.x; x++)
                for (int y = min.y; y <= max.y; y++)
                for (int z = min.z; z <= max.z; z++)
                {
                    if (!targetUnits.TryGetValue((x, y, z), out var unit))
                        continue;
                    if (!unit.objectBlocked && IsObjectMeshBlocking(mesh, unit))
                        unit.objectBlocked = true;
                    // Accumulate all bodies/Objects; one clear edge cannot undo
                    // another obstacle, and both cells are checked when linking.
                    unit.objectBlockedDirections |= GetObjectBlockingDirections(mesh, unit);
                }
            }
        }

        // Central 0.8-square standability and four 0.4-deep edge strips share the
        // same exact physical-body clipping and relative-ground height test.
        private bool IsObjectMeshBlocking(Z_Mesh.MeshInfo mesh, NavUnit unit)
        {
            float radius = ObjectNavigationHalfWidth;
            return IsObjectMeshBlockingInRect(mesh, unit, unit.realPos.x - radius, unit.realPos.x + radius,
                unit.realPos.z - radius, unit.realPos.z + radius);
        }

        private int GetObjectBlockingDirections(Z_Mesh.MeshInfo mesh, NavUnit unit)
        {
            Vector3 size = _super.data.mainData.mapUnitSize;
            float halfX = Mathf.Abs(size.x) * .5f, halfZ = Mathf.Abs(size.z) * .5f;
            float radius = ObjectNavigationHalfWidth;
            int blocked = 0;
            for (int dir = 0; dir < 4; dir++)
            {
                // The long side spans the whole Tile edge, including corners.
                float minX = unit.realPos.x - halfX, maxX = unit.realPos.x + halfX;
                float minZ = unit.realPos.z - halfZ, maxZ = unit.realPos.z + halfZ;
                switch ((Dir)dir)
                {
                    case Dir.Right: maxX = unit.realPos.x + halfX; minX = maxX - radius; break;
                    case Dir.Left: minX = unit.realPos.x - halfX; maxX = minX + radius; break;
                    case Dir.Forward: maxZ = unit.realPos.z + halfZ; minZ = maxZ - radius; break;
                    case Dir.Back: minZ = unit.realPos.z - halfZ; maxZ = minZ + radius; break;
                }
                if (IsObjectMeshBlockingInRect(mesh, unit, minX, maxX, minZ, maxZ))
                    blocked |= 1 << dir;
            }
            return blocked;
        }

        // Ground samples remain the Tile's plane, never the obstacle top.
        // Buffers are reused for central/edge checks and local/full rebuilds.
        private bool IsObjectMeshBlockingInRect(Z_Mesh.MeshInfo mesh, NavUnit unit,
            float minX, float maxX, float minZ, float maxZ)
        {
            Vector3 size = _super.data.mainData.mapUnitSize;
            float halfX = Mathf.Abs(size.x) * 0.5f, halfZ = Mathf.Abs(size.z) * 0.5f;
            float ground = unit.realPos.y, slopeX = 0f, slopeZ = 0f;
            if (unit.dirGroundY != null && unit.dirGroundY.Length >= 4)
            {
                ground = (unit.dirGroundY[0] + unit.dirGroundY[1]) * 0.5f;
                slopeX = (unit.dirGroundY[0] - unit.dirGroundY[1]) / Mathf.Max(halfX, GeometryEpsilon);
                slopeZ = (unit.dirGroundY[2] - unit.dirGroundY[3]) / Mathf.Max(halfZ, GeometryEpsilon);
            }
            if (mesh.type == Z_Mesh.MeshType.Sphere)
                return IsSphereBlocking(mesh, unit.realPos, ground, slopeX, slopeZ, minX, maxX, minZ, maxZ);
            if (mesh.type != Z_Mesh.MeshType.Cube || mesh.positions == null || mesh.positions.Length < 8)
                return false;
            var vertices = mesh.positions;
            if ((vertices[1] - vertices[0]).sqrMagnitude == 0f
                || (vertices[4] - vertices[0]).sqrMagnitude == 0f)
                return false;
            // Cheap conservative rejection before clipping, especially for low
            // bridges and neighbouring floors in the physical coverage index.
            float highestRise = float.NegativeInfinity;
            float bodyMinX = float.PositiveInfinity, bodyMaxX = float.NegativeInfinity;
            float bodyMinZ = float.PositiveInfinity, bodyMaxZ = float.NegativeInfinity;
            for (int i = 0; i < 8; i++)
            {
                var point = vertices[i];
                float floor = ground + slopeX * (point.x - unit.realPos.x) + slopeZ * (point.z - unit.realPos.z);
                highestRise = Mathf.Max(highestRise, point.y - floor);
                bodyMinX = Mathf.Min(bodyMinX, point.x);
                bodyMaxX = Mathf.Max(bodyMaxX, point.x);
                bodyMinZ = Mathf.Min(bodyMinZ, point.z);
                bodyMaxZ = Mathf.Max(bodyMaxZ, point.z);
            }
            if (highestRise <= ObjectBlockHeight + GeometryEpsilon
                || bodyMaxX <= minX || bodyMinX >= maxX || bodyMaxZ <= minZ || bodyMinZ >= maxZ)
                return false;
            for (int face = 0; face < 6; face++)
            {
                for (int i = 0; i < 4; i++) objectClipA[i] = vertices[objectBoxFaces[face, i]];
                int count = ClipObjectFace(objectClipA, 4, objectClipB, true, minX, true);
                count = ClipObjectFace(objectClipB, count, objectClipA, true, maxX, false);
                count = ClipObjectFace(objectClipA, count, objectClipB, false, minZ, true);
                count = ClipObjectFace(objectClipB, count, objectClipA, false, maxZ, false);
                if (count < 3) continue;
                // A body merely touching the cell edge does not occupy its area.
                // Work relative to the cell center to avoid large-world rounding.
                float area = 0f;
                for (int i = 0; i < count; i++)
                {
                    var a = objectClipA[i] - unit.realPos;
                    var b = objectClipA[(i + 1) % count] - unit.realPos;
                    area += a.x * b.z - b.x * a.z;
                }
                if (Mathf.Abs(area) <= GeometryEpsilon * GeometryEpsilon) continue;
                for (int i = 0; i < count; i++)
                {
                    var point = objectClipA[i];
                    float floor = ground + slopeX * (point.x - unit.realPos.x) + slopeZ * (point.z - unit.realPos.z);
                    if (point.y - floor > ObjectBlockHeight + GeometryEpsilon) return true;
                }
            }
            return false;
        }

        private static int ClipObjectFace(Vector3[] input, int count, Vector3[] output,
            bool xAxis, float boundary, bool keepGreater)
        {
            if (count == 0) return 0;
            int written = 0;
            Vector3 previous = input[count - 1];
            float previousDistance = (xAxis ? previous.x : previous.z) - boundary;
            bool previousInside = keepGreater ? previousDistance >= 0f : previousDistance <= 0f;
            for (int i = 0; i < count; i++)
            {
                var current = input[i];
                float distance = (xAxis ? current.x : current.z) - boundary;
                bool inside = keepGreater ? distance >= 0f : distance <= 0f;
                if (inside != previousInside)
                    output[written++] = Vector3.LerpUnclamped(previous, current, previousDistance / (previousDistance - distance));
                if (inside) output[written++] = current;
                previous = current;
                previousDistance = distance;
                previousInside = inside;
            }
            return written;
        }

        private static bool IsSphereBlocking(Z_Mesh.MeshInfo mesh, Vector3 center, float ground,
            float slopeX, float slopeZ, float minX, float maxX, float minZ, float maxZ)
        {
            if (mesh.positions == null || mesh.positions.Length < 6) return false;
            // Same radius as physical collision, including collisionScale.
            float radius = (mesh.positions[(int)Graph.SphereSixPoint.Right]
                - mesh.positions[(int)Graph.SphereSixPoint.Left]).magnitude * 0.5f;
            if (radius <= 0f) return false;
            float dx = Mathf.Clamp(mesh.center.x, minX, maxX) - mesh.center.x;
            float dz = Mathf.Clamp(mesh.center.z, minZ, maxZ) - mesh.center.z;
            if (dx * dx + dz * dz >= radius * radius) return false;
            // Maximize height above the ground plane over the sphere clipped by
            // the four cell sides. Each X/Z coordinate is free or on either side.
            for (int xSide = 0; xSide < 3; xSide++)
            for (int zSide = 0; zSide < 3; zSide++)
            {
                dx = xSide == 0 ? 0f : (xSide == 1 ? minX : maxX) - mesh.center.x;
                dz = zSide == 0 ? 0f : (zSide == 1 ? minZ : maxZ) - mesh.center.z;
                float remaining = radius * radius - dx * dx - dz * dz;
                if (remaining < 0f) continue;
                float directionX = xSide == 0 ? -slopeX : 0f;
                float directionZ = zSide == 0 ? -slopeZ : 0f;
                float height = Mathf.Sqrt(remaining / (1f + directionX * directionX + directionZ * directionZ));
                dx += directionX * height;
                dz += directionZ * height;
                float x = mesh.center.x + dx, z = mesh.center.z + dz;
                if (x < minX - GeometryEpsilon || x > maxX + GeometryEpsilon
                    || z < minZ - GeometryEpsilon || z > maxZ + GeometryEpsilon) continue;
                float floor = ground + slopeX * (x - center.x) + slopeZ * (z - center.z);
                if (mesh.center.y + height - floor > ObjectBlockHeight + GeometryEpsilon) return true;
            }
            return false;
        }

        private void RebuildLinks(NavUnit navUnit, HashSet<(int, int, int)> blocked)
        {
            navUnit.links.Clear();
            var pos = navUnit.pos;
            if (navUnit.objectBlocked || blocked.Contains((pos.x, pos.y, pos.z)))
                return;

            for (int m = -1; m <= 1; m++)
                for (int dir = 0; dir < tryDir.Length; dir++)
                {
                    Vector3Int linkPos = pos + tryDir[dir] + new Vector3Int(0, m, 0);
                    var key = (linkPos.x, linkPos.y, linkPos.z);
                    // Exact Tile existence already implies InArea; do not repeat
                    // the height-column lookup or fetch Tile data for every edge.
                    if (blocked.Contains(key) || !_super.data.maps.ContainsKey(key)
                        || !navUnits.TryGetValue(key, out var linkNavUnit) || linkNavUnit.objectBlocked)
                        continue;
                    if ((navUnit.objectBlockedDirections & (1 << dir)) != 0
                        || (linkNavUnit.objectBlockedDirections & (1 << (dir ^ 1))) != 0)
                        continue;
                    if (linkNavUnit.dirMaxY[dir ^ 1] - navUnit.dirMaxY[dir] < step)
                        navUnit.links.Add(linkNavUnit);
                }
        }

        private void MarkMapGroundCoveredNavUnits(TileUnitForm.Data map, HashSet<(int, int, int)> blocked)
        {
            if (map.scale == Vector3.zero || map.prefabName != MapGroundPrefabName)
                return;

            Vector3 cellSize = _super.data.mainData.mapUnitSize;
            GameObject prefab = map.unit.prefab;
            if (!mapGroundCoverageCaches.TryGetValue(map.uid, out var coverage)
                || !coverage.Matches(map, prefab, cellSize))
            {
                if (coverage != null)
                    RemoveGroundCoverage(map.uid, coverage);
                coverage = BuildMapGroundCoverage(map, prefab, cellSize);
                mapGroundCoverageCaches[map.uid] = coverage;
                RegisterGroundCandidates(map.uid, coverage);
            }

            for (int i = 0; i < coverage.coveredNavUnits.Count; i++)
                blocked.Add(coverage.coveredNavUnits[i]);
        }

        private MapGroundCoverageCache BuildMapGroundCoverage(
            TileUnitForm.Data map,
            GameObject prefab,
            Vector3 cellSize)
        {
            var coverage = new MapGroundCoverageCache
            {
                prefabName = map.prefabName,
                prefab = prefab,
                mapPos = map.mapPos,
                pos = map.pos,
                euler = map.euler,
                scale = map.scale,
                cellSize = cellSize
            };
            const float overlapEpsilon = 0.0001f;
            Vector3 halfCell = new Vector3(cellSize.x * 0.5f, 0f, cellSize.z * 0.5f);
            // Retain the owner key even when the prefab has no physical meshes,
            // so removing/replacing this Tile always discards its UID cache.
            coverage.candidateNavUnits.Add((map.mapPos.x, map.mapPos.y, map.mapPos.z));

            foreach (var mesh in map.unit.GetMeshes(CollideType.CollideOnly))
            {
                if (mesh.positions == null || mesh.positions.Length == 0)
                    continue;

                Vector3 min = mesh.positions[0];
                Vector3 max = mesh.positions[0];
                for (int i = 1; i < mesh.positions.Length; i++)
                {
                    min = Vector3.Min(min, mesh.positions[i]);
                    max = Vector3.Max(max, mesh.positions[i]);
                }

                int radiusX = Mathf.CeilToInt((max.x - min.x) / cellSize.x) + 1;
                int radiusY = Mathf.CeilToInt((max.y - min.y) / cellSize.y) + 1;
                int radiusZ = Mathf.CeilToInt((max.z - min.z) / cellSize.z) + 1;

                for (int x = map.mapPos.x - radiusX; x <= map.mapPos.x + radiusX; x++)
                for (int y = map.mapPos.y - radiusY; y <= map.mapPos.y + radiusY; y++)
                for (int z = map.mapPos.z - radiusZ; z <= map.mapPos.z + radiusZ; z++)
                {
                    var key = (x, y, z);
                    coverage.candidateNavUnits.Add(key);
                    if (key == (map.mapPos.x, map.mapPos.y, map.mapPos.z) ||
                        !navUnits.TryGetValue(key, out var navUnit))
                    {
                        continue;
                    }

                    Vector3 cellMin = navUnit.realPos - halfCell;
                    Vector3 cellMax = navUnit.realPos + halfCell + Vector3.up * cellSize.y;

                    bool overlapX = cellMax.x > min.x + overlapEpsilon && cellMin.x < max.x - overlapEpsilon;
                    bool overlapY = cellMax.y > min.y + overlapEpsilon && cellMin.y < max.y - overlapEpsilon;
                    bool overlapZ = cellMax.z > min.z + overlapEpsilon && cellMin.z < max.z - overlapEpsilon;
                    // A solid Tile can clip the bottom of a neighbouring cell
                    // without rising enough above its floor to block passage.
                    if (overlapX && overlapY && overlapZ
                        && max.y - GetNavGroundHeight(navUnit) >= PassableObstacleHeight
                        && !coverage.coveredNavUnits.Contains(key))
                        coverage.coveredNavUnits.Add(key);
                }
            }

            return coverage;
        }

        private static float GetNavGroundHeight(NavUnit unit)
        {
            if (unit.dirGroundY == null || unit.dirGroundY.Length == 0)
                return unit.realPos.y;

            float lowestGround = unit.dirGroundY[0];
            for (int i = 1; i < unit.dirGroundY.Length; i++)
                lowestGround = Mathf.Min(lowestGround, unit.dirGroundY[i]);
            return lowestGround;
        }




        public Vector3 GetNormalWithoutY(Vector3 tar)
        {
            tar.y = 0;
            return tar.normalized;
        }
        private readonly Dictionary<(int, int), List<Vector2Int>> clearanceOffsetCache =
            new Dictionary<(int, int), List<Vector2Int>>();

        public IReadOnlyList<Vector2Int> GetClearanceOffsets(float agentRadius)
        {
            Vector3 cellSize = _super.data.mainData.mapUnitSize;
            float cellX = Mathf.Max(0.0001f, Mathf.Abs(cellSize.x));
            float cellZ = Mathf.Max(0.0001f, Mathf.Abs(cellSize.z));
            int radiusX = agentRadius <= cellX * 0.5f + 0.0001f
                ? 0
                : Mathf.CeilToInt((agentRadius - cellX * 0.5f) / cellX);
            int radiusZ = agentRadius <= cellZ * 0.5f + 0.0001f
                ? 0
                : Mathf.CeilToInt((agentRadius - cellZ * 0.5f) / cellZ);
            var key = (radiusX, radiusZ);
            if (!clearanceOffsetCache.TryGetValue(key, out var offsets))
            {
                offsets = new List<Vector2Int>();
                for (int x = -radiusX; x <= radiusX; x++)
                for (int z = -radiusZ; z <= radiusZ; z++)
                    offsets.Add(new Vector2Int(x, z));
                clearanceOffsetCache[key] = offsets;
            }
            return offsets;
        }

        public bool TryGetOffsetUnit(NavUnit center, Vector2Int offset, out NavUnit unit)
        {
            return navUnits.TryGetValue(
                (center.pos.x + offset.x, center.pos.y, center.pos.z + offset.y),
                out unit);
        }

        /// <summary>
        /// 检查导航节点本身是否可走，不考虑角色的 passType 能力。
        /// </summary>
        public bool IsBaseWalkable(int x, int y, int z)
        {
            return navUnits != null
                && navUnits.TryGetValue((x, y, z), out var unit)
                && IsBaseWalkable(unit);
        }

        internal bool IsBaseWalkable(NavUnit unit)
        {
            // An isolated but unobstructed center is standable; missing edges
            // are connectivity, not a blocked-start/blocked-target exception.
            return unit != null && !unit.isNull && !unit.objectBlocked && unit.links != null
                && !mapGroundBlocked.Contains((unit.pos.x, unit.pos.y, unit.pos.z));
        }

#if UNITY_EDITOR
        /// <summary>读取现有导航图；有玩家时复用 BFS 的体型和 passType 判断。</summary>
        public void DrawWalkableTilesDebug(CharacterUnit character)
        {
            if (!_super.enable || _super.data == null || navUnits == null || !(bfs is Bfs search))
                return;

            var offsets = GetClearanceOffsets(character == null ? 0f : character.GetNavigationRadius());
            var halfSize = _super.data.mainData.mapUnitSize * 0.5f;
            float halfX = Mathf.Abs(halfSize.x), halfZ = Mathf.Abs(halfSize.z);
            var previousColor = Gizmos.color;
            var previousMatrix = Gizmos.matrix;
            try
            {
                Gizmos.color = Color.green;
                Gizmos.matrix = Matrix4x4.identity;
                foreach (var unit in navUnits.Values)
                {
                    bool walkable = character == null
                        ? IsBaseWalkable(unit.pos.x, unit.pos.y, unit.pos.z)
                        : search.IsWalkable(unit, offsets, character.passTypes);
                    if (!walkable)
                        continue;

                    float ground = unit.realPos.y, slopeX = 0f, slopeZ = 0f;
                    if (unit.dirGroundY != null && unit.dirGroundY.Length >= 4)
                    {
                        ground = (unit.dirGroundY[0] + unit.dirGroundY[1]) * 0.5f;
                        slopeX = (unit.dirGroundY[0] - unit.dirGroundY[1]) / (offset[0].x - offset[1].x);
                        slopeZ = (unit.dirGroundY[2] - unit.dirGroundY[3]) / (offset[2].y - offset[3].y);
                    }
                    var center = new Vector3(unit.realPos.x, ground + 0.1f, unit.realPos.z);
                    var xSide = new Vector3(halfX, slopeX * halfX, 0f);
                    var zSide = new Vector3(0f, slopeZ * halfZ, halfZ);
                    var a = center - xSide - zSide;
                    var b = center + xSide - zSide;
                    var c = center + xSide + zSide;
                    var d = center - xSide + zSide;
                    Gizmos.DrawLine(a, b);
                    Gizmos.DrawLine(b, c);
                    Gizmos.DrawLine(c, d);
                    Gizmos.DrawLine(d, a);
                }
            }
            finally
            {
                Gizmos.color = previousColor;
                Gizmos.matrix = previousMatrix;
            }
        }
#endif

        public Vector3 GetNextDir(Vector3 cur, Vector3 tar, int maxStep, float agentRadius,
            IReadOnlyCollection<int> passTypes, NavigationEndpointState endpointState = null)
        {
            var res = bfs.GetNextDir(cur, tar, maxStep, agentRadius, passTypes, endpointState);
            res.y = 0;
            return res;
        }

        public bool TryGetGroundUnit(Vector3 position, out NavUnit unit)
        {
            unit = null;
            if (navUnits == null)
                return false;
            var pos = RealPos2MapPosInt(position);
            if (navUnits.TryGetValue((pos.x, pos.y, pos.z), out unit) && !unit.isNull)
                return true;

            // 只寻找同列下方地面，不能先把空格/阻断格映射到另一列。
            unit = null;
            if (_super.data.mapXZ2Y.TryGetValue((pos.x, pos.z), out var heights))
                foreach (int y in heights)
                    if (y < pos.y && navUnits.TryGetValue((pos.x, y, pos.z), out var floor)
                        && !floor.isNull && (unit == null || y > unit.pos.y))
                        unit = floor;
            return unit != null;
        }
        public Vector3Int RealPos2MapPosInt(Vector3 pos)
        {
            return _super.utilCtrl.RealPos2MapPosInt(pos);
        }
        public Vector3Int GetClosestExistInArea(Vector3Int pos)
        {
            return _super.utilCtrl.GetClosestExistInArea(pos);
        }
        public bool InArea(Vector3Int pos)
        {
            return _super.utilCtrl.InArea(pos);
        }
        public Vector3 MapPos2RealPos(Vector3Int pos)
        {
            return _super.utilCtrl.MapPos2RealPos(pos);
        }
        public Dir GetDir(Vector3 self, Vector3 tar)
        {
            if (self.x < tar.x - 0.01f)
                return Dir.Right;
            if (self.x > tar.x + 0.01f)
                return Dir.Left;
            if (self.z < tar.z - 0.01f)
                return Dir.Forward;
            return Dir.Back;
        }
    }

}
