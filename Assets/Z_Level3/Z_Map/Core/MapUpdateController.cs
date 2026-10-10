using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Z_Debug;
using Z_DesignStyle;
using Z_Map.Analysis;
using Z_Map.Form;
using Z_Math;
using Z_Mesh;
using Z_Time;
using Z_UnitSystem;
using Z_UnitSystem.Form;
using static Z_DesignStyle.Z_DoubleDictionary;
using static Z_Math.Graph;
using Mesh = Z_Mesh.Mesh;
namespace Z_Map
{

    public class MapUpdateController : Z_Controller<MapManager>
    {
        public MapUpdateController(MapManager super) : base(super)
        {
        }
        public Vector3 curCenterPos;
        private Vector3 lastCenterPos;
        private Vector3Int viewCenter;

        public DoubleDictionary<ObjectUnit, TileUnit> objectTileDic = new DoubleDictionary<ObjectUnit, TileUnit>();
        private readonly DoubleDictionary<ObjectUnit, TileUnit> objectPassTypeTileDic = new DoubleDictionary<ObjectUnit, TileUnit>();
        internal readonly DoubleDictionary<ObjectUnit, TileUnit> objectNavigationTileDic = new DoubleDictionary<ObjectUnit, TileUnit>();
        public DoubleDictionary<CharacterUnit, TileUnit> characterTileDic = new DoubleDictionary<CharacterUnit, TileUnit>();
        public DoubleDictionary<CharacterUnit, TileUnit> characterOverlapTileDic = new DoubleDictionary<CharacterUnit, TileUnit>();
        public DoubleDictionary<ItemUnit, TileUnit> itemTileDic = new DoubleDictionary<ItemUnit, TileUnit>();
        private readonly DoubleDictionary<MapUnit, (int, int)> unitColumns = new DoubleDictionary<MapUnit, (int, int)>();

        public void ForgetUnitColumns(MapUnit unit) => unitColumns.Del(unit);

        public void CollectHistoryColumn(Vector3Int pos, HashSet<MapUnit> units)
        {
            if (unitColumns.TryGet((pos.x, pos.z), out var candidates))
                foreach (var unit in candidates)
                    if (UnitForm.DataByUid.TryGetValue(unit.data.uid, out var current)
                        && ReferenceEquals(current, unit.data))
                        units.Add(unit);
        }

        private void RefreshUnitColumns(MapUnit unit)
        {
            var anchor = _super.utilCtrl.RealPos2MapPosInt(unit.data.pos);
            if (unit is ItemUnit)
            {
                unitColumns.Move(unit, (anchor.x, anchor.z));
                return;
            }
            var bounds = new Bounds(unit.data.pos, Vector3.zero);
            if (unit is ObjectUnit obj)
                _super.utilCtrl.TryGetVisionBounds(obj.data, out bounds);
            foreach (var mesh in unit.GetMeshes(unit is CharacterUnit ? CollideType.All : CollideType.CollideOnly))
                foreach (var point in mesh.positions)
                    bounds.Encapsulate(point);
            var low = _super.utilCtrl.RealPos2MapPos(bounds.min);
            var high = _super.utilCtrl.RealPos2MapPos(bounds.max);
            unitColumns.Del(unit);
            unitColumns.Add(unit, (anchor.x, anchor.z));
            for (int x = Mathf.CeilToInt(low.x - 0.5f); x <= Mathf.FloorToInt(high.x + 0.5f); x++)
            for (int z = Mathf.CeilToInt(low.z - 0.5f); z <= Mathf.FloorToInt(high.z + 0.5f); z++)
                unitColumns.Add(unit, (x, z));
        }

        public void DetachHistoryTile(TileUnit tile)
        {
            characterTileDic.Del(tile);
            characterOverlapTileDic.Del(tile);
            itemTileDic.Del(tile);
            objectTileDic.Del(tile);
            objectPassTypeTileDic.Del(tile);
            objectNavigationTileDic.Del(tile);
            curTileLst.Remove(tile.data);
        }

        // Rebind only an affected unit, without movement/clamping or global view
        // refresh. Candidate columns remain populated even when a Tile is absent.
        public void RefreshHistoryUnit(MapUnit unit)
        {
            if (unit is ObjectUnit obj)
            {
                RefreshObjectOverlap(obj);
                RefreshHistoryVisibility(obj, objectTileDic, curObjectLst, obj.data);
                Z_EventHelper.Invoke(new ObjectEvent { type = MapEventType.Refresh, unit = obj });
                return;
            }
            var pos = _super.utilCtrl.RealPos2MapPosInt(unit.data.pos);
            var owner = _super.utilCtrl.GetTile(pos.x, pos.y, pos.z);
            if (unit is CharacterUnit character)
            {
                characterTileDic.Del(character);
                if (owner != null) characterTileDic.Add(character, owner);
                RefreshCharacterOverlap(character);
                RefreshHistoryVisibility(character, characterTileDic, curCharacterLst, character.data);
            }
            else if (unit is ItemUnit item)
            {
                itemTileDic.Del(item);
                if (owner != null) itemTileDic.Add(item, owner);
                RefreshUnitColumns(item);
                RefreshHistoryVisibility(item, itemTileDic, curItemLst, item.data);
            }
        }

        private void RefreshHistoryVisibility<T, TData>(T unit, DoubleDictionary<T, TileUnit> index,
            HashSet<TData> visible, TData data) where T : MapUnit
        {
            if (HasShowingTile(unit, index))
            {
                unit.Show();
                visible.Add(data);
                var owner = unit.belongTile;
                float degree = !DynamicGlobalSettings.playing && GlobalSettings.MOD_HIGH_LAYER_HALF_TRANSPARENT
                    && owner != null && owner.data.mapPos.y > _super.utilCtrl.RealPos2MapPosInt(curCenterPos).y ? 0.5f : 1f;
                ApplyUnitVision(unit, degree);
            }
            else
            {
                unit.Hide();
                visible.Remove(data);
            }
        }

        public void RefreshHistoryTiles(IEnumerable<Vector3Int> positions)
        {
            var refreshed = new HashSet<Vector3Int>();
            foreach (var pos in positions)
                for (int x = pos.x - 1; x <= pos.x + 1; x++)
                for (int z = pos.z - 1; z <= pos.z + 1; z++)
                {
                    var p = new Vector3Int(x, pos.y, z);
                    if (!refreshed.Add(p) || !_super.data.maps.TryGetValue((x, pos.y, z), out var tile))
                        continue;
                    var bounds = GetLayerView(lastView, p.y, DynamicGlobalSettings.cameraMode == CameraMode.Isometric);
                    bool inView = hasView && p.y >= lastView.Item3 && p.y < lastView.Item4
                        && p.x >= bounds.minX && p.x < bounds.maxX && p.z >= bounds.minZ && p.z < bounds.maxZ;
                    if (inView)
                    {
                        bool showing = tile.unit.isShowing;
                        tile.unit.Show();
                        curTileLst.Add(tile);
                        if (showing) Z_EventHelper.Invoke(new TileEvent { type = MapEventType.Show, unit = tile.unit });
                        float degree = !DynamicGlobalSettings.playing && GlobalSettings.MOD_HIGH_LAYER_HALF_TRANSPARENT
                            && p.y > _super.utilCtrl.RealPos2MapPosInt(curCenterPos).y ? 0.5f : 1f;
                        ApplyUnitVision(tile.unit, degree);
                    }
                    else
                    {
                        tile.unit.Hide();
                        curTileLst.Remove(tile);
                    }
                }
        }
        public HashSet<TileUnitForm.Data> curTileLst
        {
            get;
            private set;
        }
        = new HashSet<TileUnitForm.Data>();
        public HashSet<TileUnitForm.Data> newMapLst
        {
            get;
            private set;
        }
        = new HashSet<TileUnitForm.Data>();
        public HashSet<TileUnitForm.Data> delMapLst
        {
            get;
            private set;
        }
        = new HashSet<TileUnitForm.Data>();
        public HashSet<ObjectUnitForm.Data> curObjectLst
        {
            get;
            private set;
        }
        = new HashSet<ObjectUnitForm.Data>();
        public HashSet<CharacterUnitForm.Data> curCharacterLst
        {
            get;
            private set;
        }
        = new HashSet<CharacterUnitForm.Data>();
        public HashSet<ItemUnitForm.Data> curItemLst
        {
            get;
            private set;
        }
        = new HashSet<ItemUnitForm.Data>();
        public (int, int, int, int, int, int) lastView
        {
            get;
            private set;
        }
        private bool hasView;
        private bool lastViewIsometric;

        private static (int minX, int maxX, int minZ, int maxZ) GetLayerView(
            (int, int, int, int, int, int) view, int layerY, bool isometric)
        {
            int centerY = view.Item3 + (view.Item4 - view.Item3) / 2;
            int offsetZ = isometric ? Mathf.Max(0, centerY - layerY) : 0;
            return (view.Item1, view.Item2, view.Item5 + offsetZ, view.Item6 + offsetZ);
        }

        private void FreshMap(bool forceFullScan)
        {
            var viewSize = _super.data.mainData.viewSize;
            var curView = (viewCenter.x - viewSize.x, viewCenter.x + viewSize.x, viewCenter.y - viewSize.y, viewCenter.y + viewSize.y, viewCenter.z - viewSize.z, viewCenter.z + viewSize.z);
            bool isometric = DynamicGlobalSettings.cameraMode == CameraMode.Isometric;
            newMapLst.Clear();
            delMapLst.Clear();
            if (!forceFullScan && hasView && curView.Equals(lastView) && isometric == lastViewIsometric)
                return;

            // Remove only Tiles that actually left this view (or were replaced in-place).
            foreach (var map in curTileLst)
            {
                var layerView = GetLayerView(curView, map.mapPos.y, isometric);
                if (map.mapPos.x >= curView.Item2 || map.mapPos.x < curView.Item1
                    || map.mapPos.y >= curView.Item4 || map.mapPos.y < curView.Item3
                    || map.mapPos.z >= layerView.maxZ || map.mapPos.z < layerView.minZ
                    || !_super.data.maps.TryGetValue((map.mapPos.x, map.mapPos.y, map.mapPos.z), out var current)
                    || !ReferenceEquals(current, map))
                {
                    map.unit.Hide();
                    delMapLst.Add(map);
                }
            }
            curTileLst.ExceptWith(delMapLst);

            // Each lower layer has its own projected rectangle. Compare it with
            // that same layer's previous rectangle, including camera height changes.
            for (int y = curView.Item3; y < curView.Item4; y++)
            {
                var current = GetLayerView(curView, y, isometric);
                var previous = GetLayerView(lastView, y, lastViewIsometric);
                bool overlaps = hasView && y >= lastView.Item3 && y < lastView.Item4
                    && current.minX < previous.maxX && current.maxX > previous.minX
                    && current.minZ < previous.maxZ && current.maxZ > previous.minZ;
                if (forceFullScan || !overlaps)
                {
                    ShowAndAddLst(curTileLst, current.minX, current.maxX, y, y + 1, current.minZ, current.maxZ);
                    continue;
                }

                // Four non-overlapping entering strips; unchanged layers do no queries.
                int commonMinX = Mathf.Max(current.minX, previous.minX);
                int commonMaxX = Mathf.Min(current.maxX, previous.maxX);
                if (current.minX < previous.minX)
                    ShowAndAddLst(curTileLst, current.minX, previous.minX, y, y + 1, current.minZ, current.maxZ);
                if (current.maxX > previous.maxX)
                    ShowAndAddLst(curTileLst, previous.maxX, current.maxX, y, y + 1, current.minZ, current.maxZ);
                if (current.minZ < previous.minZ)
                    ShowAndAddLst(curTileLst, commonMinX, commonMaxX, y, y + 1, current.minZ, previous.minZ);
                if (current.maxZ > previous.maxZ)
                    ShowAndAddLst(curTileLst, commonMinX, commonMaxX, y, y + 1, previous.maxZ, current.maxZ);
            }

            foreach (var newMap in newMapLst)
            {
                UpdateRelatedUnit(newMap.unit);

            }
            foreach (var delMap in delMapLst)
            {
                UpdateRelatedUnit(delMap.unit);

            }
            lastView = curView;
            lastViewIsometric = isometric;
            hasView = true;
        }
        private void UpdateRelatedUnit(TileUnit tile)
        {
            foreach (var ch in characterTileDic.Get(tile))
            {
                if (HasShowingTile(ch, characterTileDic))
                {
                    ch.Show();
                    curCharacterLst.Add(ch.data);
                }
                else
                {
                    ch.Hide();
                    curCharacterLst.Remove(ch.data);
                }
            }
            foreach (var item in itemTileDic.Get(tile))
            {
                if (HasShowingTile(item, itemTileDic))
                {
                    item.Show();
                    curItemLst.Add(item.data);
                }
                else
                {
                    item.Hide();
                    curItemLst.Remove(item.data);
                }
            }

            foreach (var obj in objectTileDic.Get(tile))
            {
                if (HasShowingTile(obj, objectTileDic))
                {
                    obj.Show();
                    curObjectLst.Add(obj.data);
                }
                else
                {
                    obj.Hide();
                    curObjectLst.Remove(obj.data);
                }
            }

        }
        private static bool HasShowingTile<T>(T unit, DoubleDictionary<T, TileUnit> tileDictionary)
        {
            if (!tileDictionary.TryGet(unit, out var tiles))
                return false;
            foreach (var tile in tiles)
                if (tile.isShowing)
                    return true;
            return false;
        }
        private void ShowAndAddLst(HashSet<TileUnitForm.Data> lst, int minX, int maxX, int minY, int maxY, int minZ, int maxZ)
        {
            for (int i = minX; i < maxX; i++)
            {

                for (int k = minZ; k < maxZ; k++)
                {
                    if (!_super.data.mapXZ2Y.TryGetValue((i, k), out var yLevels)
                        || yLevels.Count == 0)
                        continue;

                    // Height columns are sparse after erase operations. Enumerate
                    // only registered levels instead of assuming Min..Max is dense.
                    foreach (int j in yLevels)
                    {
                        if (j < minY)
                            continue;
                        if (j >= maxY)
                            break;
                        if (!_super.data.maps.TryGetValue((i, j, k), out var map) || map == null)
                            continue;

                        if (lst.Add(map))
                        {
                            map.unit.Show();
                            newMapLst.Add(map);
                        }
                    }
                }
            }


        }
        public void UpdateSingleOne(MapUnit unit)
        {
            // Also covers replacement of Collider data under the same prefab name.
            unit.InvalidateCollisionGeometry();
            if (unit is TileUnit tl)
            {
                // Only the edited Tile may have switched prefab. Neighbouring
                // WangTile appearance refreshes retain their live instances.
                tl.Hide();
                _super.navigationCtrl?.RefreshTerrainTiles(new[] { tl.data.mapPos });
                UpdateTileNeighbours(tl.data.mapPos);
                      
            }
            else if (unit is CharacterUnit ch)
            {
                foreach (var tile in characterTileDic.Get(ch))
                {
                    if (tile.isShowing)
                    {
                        ch.Show();
                        curCharacterLst.Add(ch.data);
                    }
                }
            }
            else if (unit is ItemUnit it)
            {
                RefreshUnitColumns(it);
                foreach (var tile in itemTileDic.Get(it))
                {
                    if (tile.isShowing)
                    {
                        it.Show();
                        curItemLst.Add(it.data);
                    }
                }
            }
            else if (unit is ObjectUnit ob)
            {
                RefreshObjectOverlap(ob);
                foreach (var tile in objectTileDic.Get(ob))
                {
                    if (tile.isShowing)
                    {
                        ob.Show();
                        curObjectLst.Add(ob.data);
                    }
                }
                Z_EventHelper.Invoke(new ObjectEvent
                {
                    type = MapEventType.Refresh,
                    unit = ob
                });
            }

        }

        public void UpdateTileNeighbours(Vector3Int mapPos)
        {
            RefreshHistoryTiles(new[] { mapPos });
            var units = new HashSet<MapUnit>();
            CollectHistoryColumn(mapPos, units);
            foreach (var unit in units) RefreshHistoryUnit(unit);
        }

        private void UpdateMapInfo(bool simulateCharacters)
        {
            //return;
            //update
            switch (GlobalSettings.UPDATE_TILE_TYPE)
            {
                case GlobalSettings.UpdateType.ShowOnly:
                    foreach (var tile in curTileLst)
                    {
                        tile.unit.UpdateInfo();
                    }
                    break;
                case GlobalSettings.UpdateType.All:

                    foreach (var tile in TileUnitForm.DataByUid.Values)
                    {
                        tile.unit.UpdateInfo();
                    }
                    break;
                case GlobalSettings.UpdateType.None:
                default:
                    break;
            }
            switch (GlobalSettings.UPDATE_OBJECT_TYPE)
            {
                case GlobalSettings.UpdateType.ShowOnly:
                    foreach (var obj in curObjectLst)
                    {
                        obj.unit.UpdateInfo();
                    }
                    break;
                case GlobalSettings.UpdateType.All:

                    var lst = objectTileDic.GetDicT1().Keys.ToList();
                    foreach (var oUnit in lst)
                    {
                        oUnit.UpdateInfo();
                    }
                    break;
                case GlobalSettings.UpdateType.None:
                default:
                    break;
            }

            switch (GlobalSettings.UPDATE_CHARACTER_TYPE)
            {
                case GlobalSettings.UpdateType.ShowOnly:
                    foreach (var obj in curCharacterLst)
                    {
                        obj.unit.UpdateInfo(simulateCharacters);
                    }
                    break;
                case GlobalSettings.UpdateType.All:
                    var lst = characterTileDic.GetDicT1().Keys.ToList();
                    foreach (var cUnit in lst)
                    {
                        cUnit.UpdateInfo(simulateCharacters);
                    }
                    break;
                case GlobalSettings.UpdateType.None:
                default:
                    break;
            }

        }

        private HashSet<(int, int, int)> bfsVisited = new HashSet<(int, int, int)>();
        private Queue<(int, int)> bfsQueue = new Queue<(int, int)>();
        private const float FullHighLayerOcclusionDegree = 0f;
        private const float HalfHighLayerOcclusionDegree = 0.5f;
        private const int HalfHighLayerOcclusionRadius = 3;
        private const int HalfHighLayerOcclusionRadiusSqr =
            HalfHighLayerOcclusionRadius * HalfHighLayerOcclusionRadius;
        private const float CurrentLayerObjectOcclusionDegree = 0.5f;
        private const int ObjectOcclusionHalfWidth = 3;
        private const int OcclusionBackwardDistance = 5;
        private const int OcclusionHeight = 5;
        private const float ProjectedUpperTileColliderHeight = 0.5f;
        private static readonly int FadeCenterRangeProperty = Shader.PropertyToID("_MapFadeCenterRange");
        private static readonly int FadeCenterPositionProperty = Shader.PropertyToID("_MapFadeCenterPosition");
        private Dictionary<MapUnit, float> previousVision = new Dictionary<MapUnit, float>();
        private Dictionary<MapUnit, float> nextVision = new Dictionary<MapUnit, float>();
        private readonly Dictionary<TileUnit, float> frontVisionOverrides = new Dictionary<TileUnit, float>();
        private readonly HashSet<ObjectUnit> nearbyOccludingObjects = new HashSet<ObjectUnit>();
        private readonly Dictionary<ObjectUnit, float> highLayerOcclusionObjects = new Dictionary<ObjectUnit, float>();
        private readonly Dictionary<ObjectUnit, Vector2> objectProjectionRanges = new Dictionary<ObjectUnit, Vector2>();
        private readonly Dictionary<ObjectUnit, bool> objectHalfOcclusionEligibility = new Dictionary<ObjectUnit, bool>();
        private readonly Dictionary<(int, int), bool> projectedUpperTileChecks = new Dictionary<(int, int), bool>();
        private readonly HashSet<TileUnit> characterOverlapBuffer = new HashSet<TileUnit>();

        private static bool IsWithinHalfOcclusionRange(int x, int z, Vector3Int center)
        {
            // BFS passes the source footprint; direct-hit fallback passes its player-layer projection.
            int offsetX = x - center.x;
            int offsetZ = z - center.z;
            return offsetX * offsetX + offsetZ * offsetZ <= HalfHighLayerOcclusionRadiusSqr;
        }

        /// <summary>
        /// 在当前视野内广搜起点相连的高层 Tile；半透明只应用到玩家周围的圆形范围。
        /// </summary>
        private void BfsLayerVision(
            int layerY,
            int centerX,
            int centerZ,
            float degree,
            Vector3Int playerMapPos)
        {
            if (!_super.utilCtrl.ContainsTile(centerX, layerY, centerZ))
                return;

            var viewSize = _super.data.mainData.viewSize;
            var start = (centerX, layerY, centerZ);
            if (bfsVisited.Contains(start))
                return;
            bfsVisited.Add(start);
            bfsQueue.Enqueue((centerX, centerZ));

            while (bfsQueue.Count > 0)
            {
                var cur = bfsQueue.Dequeue();
                var map = _super.utilCtrl.GetTileData(cur.Item1, layerY, cur.Item2);
                bool withinHalfTransparentRange = IsWithinHalfOcclusionRange(cur.Item1, cur.Item2, playerMapPos);
                if (degree == FullHighLayerOcclusionDegree || withinHalfTransparentRange)
                {
                    CollectHighTileVision(map.unit, degree, playerMapPos);
                }

                for (int x = cur.Item1 - 1; x <= cur.Item1 + 1; x += 2)
                {
                    if (x < viewCenter.x - viewSize.x || x >= viewCenter.x + viewSize.x) continue;
                    if (!bfsVisited.Contains((x, layerY, cur.Item2)) && _super.utilCtrl.ContainsTile(x, layerY, cur.Item2))
                    {
                        bfsVisited.Add((x, layerY, cur.Item2));
                        bfsQueue.Enqueue((x, cur.Item2));
                    }
                }
                for (int z = cur.Item2 - 1; z <= cur.Item2 + 1; z += 2)
                {
                    if (z < viewCenter.z - viewSize.z || z >= viewCenter.z + viewSize.z) continue;
                    if (!bfsVisited.Contains((cur.Item1, layerY, z)) && _super.utilCtrl.ContainsTile(cur.Item1, layerY, z))
                    {
                        bfsVisited.Add((cur.Item1, layerY, z));
                        bfsQueue.Enqueue((cur.Item1, z));
                    }
                }
            }
        }

        private bool HasTallProjectedUpperTile(int x, int playerLayer, int z)
        {
            // All callers share the same player layer during this visibility pass.
            var key = (x, z);
            if (!projectedUpperTileChecks.TryGetValue(key, out bool tall))
            {
                tall = CheckTallProjectedUpperTile(x, playerLayer, z);
                projectedUpperTileChecks[key] = tall;
            }
            return tall;
        }

        private bool CheckTallProjectedUpperTile(int x, int playerLayer, int z)
        {
            // Check exactly the layer above the player-layer projection, not
            // the original high Tile's layer or a nearest/fallback ground Tile.
            var upperTile = _super.utilCtrl.GetTileData(x, playerLayer + 1, z);
            if (upperTile == null)
                return false;

            foreach (var body in upperTile.unit.GetMeshes(CollideType.CollideOnly))
            {
                var points = body.positions;
                if (points == null || points.Length == 0)
                    continue;
                // Sphere collision uses the Right/Left diameter regardless of
                // rotation; its six samples are not the world's Y extrema.
                if (body.type == MeshType.Sphere && points.Length >= 6)
                {
                    float diameter = (points[(int)SphereSixPoint.Right] - points[(int)SphereSixPoint.Left]).magnitude;
                    if (diameter > ProjectedUpperTileColliderHeight + 0.0001f)
                        return true;
                    continue;
                }
                float minY = points[0].y;
                float maxY = minY;
                for (int i = 1; i < points.Length; i++)
                {
                    minY = Mathf.Min(minY, points[i].y);
                    maxY = Mathf.Max(maxY, points[i].y);
                }
                // Cached world geometry includes root/child scale and rotation.
                // A small tolerance keeps exactly .5 tall bodies from roundoff.
                if (maxY - minY > ProjectedUpperTileColliderHeight + 0.0001f)
                    return true;
            }
            return false;
        }

        private void CollectHighLayerObjectCandidates(TileUnit tile, float degree)
        {
            if (objectTileDic.TryGet(tile, out var objects))
                foreach (var unit in objects)
                    if (!highLayerOcclusionObjects.TryGetValue(unit, out var oldDegree) || degree < oldDegree)
                        highLayerOcclusionObjects[unit] = degree;
        }

        private void CollectHighTileVision(TileUnit tile, float degree, Vector3Int center)
        {
            int projectedZ = tile.data.mapPos.z + tile.data.mapPos.y - center.y;
            // Collider exceptions win over both direct hits and connected-component hiding.
            if (HasTallProjectedUpperTile(tile.data.mapPos.x, center.y, projectedZ))
            {
                nextVision[tile] = 1f;
                return;
            }
            if (nextVision.TryGetValue(tile, out float oldDegree))
                degree = Mathf.Min(degree, oldDegree);
            nextVision[tile] = degree;
            if (HasTallProjectedUpperTile(tile.data.mapPos.x, center.y, projectedZ - 1))
                frontVisionOverrides[tile] = 1f;
            CollectHighLayerObjectCandidates(tile, degree);
        }

        private bool CanHalfOccludeObject(ObjectUnit unit, int currentLayer)
        {
            if (!objectHalfOcclusionEligibility.TryGetValue(unit, out bool eligible))
            {
                eligible = CheckObjectHalfOcclusionEligibility(unit, currentLayer);
                objectHalfOcclusionEligibility[unit] = eligible;
            }
            return eligible;
        }

        private bool CheckObjectHalfOcclusionEligibility(ObjectUnit unit, int currentLayer)
        {
            // Classify the root, not an upper Tile overlapped by a current/lower Object.
            int layer = _super.utilCtrl.RealPos2MapPosInt(unit.data.pos).y;
            if (layer != currentLayer)
                return layer > currentLayer;
            // Collider meshes already include colliderScale and any fitted texture bounds.
            foreach (var body in unit.GetMeshes(CollideType.CollideOnly))
            {
                var points = body.positions;
                // Physical Spheres use one radius, so Y equals Z and never passes strict Y > Z.
                if (body.type != MeshType.Cube || points == null || points.Length < 8)
                    continue;
                // colliderScale=0 leaves Y intact but collapses X/Z. Such a
                // body has no footprint and must not qualify merely because Y > 0.
                // Check its own edges: rotation can give a collapsed box a nonzero AABB.
                if ((points[1] - points[0]).sqrMagnitude == 0f
                    || (points[4] - points[0]).sqrMagnitude == 0f)
                    continue;
                float minY = points[0].y, maxY = minY;
                float minZ = points[0].z, maxZ = minZ;
                for (int i = 1; i < points.Length; i++)
                {
                    minY = Mathf.Min(minY, points[i].y);
                    maxY = Mathf.Max(maxY, points[i].y);
                    minZ = Mathf.Min(minZ, points[i].z);
                    maxZ = Mathf.Max(maxZ, points[i].z);
                }
                // Compare each actual body, not a union inflated by gaps or Triggers.
                if (maxY - minY > maxZ - minZ + .0001f)
                    return true;
            }
            return false;
        }

        private bool ProjectsForNearbyOcclusion(ObjectUnit unit, TileUnit tile, float halfDepth, Vector3Int center)
        {
            if (!CanHalfOccludeObject(unit, center.y))
                return false;
            if (!objectProjectionRanges.TryGetValue(unit, out var projectedZ))
            {
                projectedZ = _super.utilCtrl.GetVisionProjectionZRange(unit.data);
                objectProjectionRanges[unit] = projectedZ;
            }
            // Higher Objects retain the original player-plane projection rule.
            // Classify the Object itself, not a higher Tile overlapped by a current-layer Object.
            if (_super.utilCtrl.RealPos2MapPosInt(unit.data.pos).y > center.y)
                return projectedZ.y > curCenterPos.z + curCenterPos.y + .0001f;

            // Only current-layer Objects use the enumerated Tile's front edge.
            return projectedZ.y > tile.data.pos.z + tile.data.pos.y + halfDepth + .0001f;
        }

        private float GetObjectOcclusionDegree(ObjectUnit unit, float degree, int currentLayer,
            bool isSideView, bool modHighLayerHalfTransparent)
        {
            // All automatic half-opacity paths share this gate; full hiding is unchanged.
            if (degree == CurrentLayerObjectOcclusionDegree && !CanHalfOccludeObject(unit, currentLayer))
                return 1f;
            if (isSideView && !modHighLayerHalfTransparent && degree == CurrentLayerObjectOcclusionDegree
                && !nearbyOccludingObjects.Contains(unit))
                return 1f;
            return degree;
        }

        private void CollectHighLayerObjectOcclusion(int currentLayer, bool isSideView, bool modHighLayerHalfTransparent)
        {
            foreach (var occlusion in highLayerOcclusionObjects)
            {
                var unit = occlusion.Key;
                // Independent nearby occlusion takes priority, even over a fully hidden owner.
                if (nearbyOccludingObjects.Contains(unit))
                    continue;
                float degree = occlusion.Value;
                if (objectTileDic.TryGet(unit, out var tiles))
                {
                    foreach (var tile in tiles)
                    {
                        if (tile.data.mapPos.y != currentLayer)
                            continue;
                        // Visual coverage of the current layer wins, regardless of owner/link order.
                        degree = CurrentLayerObjectOcclusionDegree;
                        break;
                    }
                }
                nextVision[unit] = GetObjectOcclusionDegree(unit, degree, currentLayer, isSideView, modHighLayerHalfTransparent);
            }
        }

        private bool IsOccludingHighTile(int x, int layerY, int z, Vector3Int center)
        {
            return _super.utilCtrl.ContainsTile(x, layerY, z)
                && !HasTallProjectedUpperTile(x, center.y, z + layerY - center.y);
        }

        private bool ShouldFullyOccludeHighLayer(int layerY, Vector3Int center)
        {
            return IsOccludingHighTile(center.x, layerY, center.z, center);
        }

        private static bool IsObjectAboveTile(ObjectUnit unit, TileUnit tile)
        {
            // The current layer occupies the first world unit above the Tile.
            // A raised body still inside that layer must not count as a roof.
            float layerUpperY = tile.data.pos.y + 1f;
            var bodies = unit.GetMeshes(CollideType.CollideOnly);
            if (bodies.Count == 0)
                bodies = unit.GetMeshes(CollideType.TriggerOnly);

            bool hasBounds = false;
            foreach (var body in bodies)
            {
                var points = body.positions;
                if (points == null || points.Length == 0)
                    continue;
                float minY;
                if (body.type == MeshType.Sphere && points.Length >= 6)
                {
                    // Match physical collision radius; rotated sphere samples
                    // are not the world's lower bound.
                    float radius = (points[(int)SphereSixPoint.Right]
                        - points[(int)SphereSixPoint.Left]).magnitude * .5f;
                    minY = body.center.y - radius;
                }
                else
                {
                    minY = points[0].y;
                    for (int i = 1; i < points.Length; i++)
                        minY = Mathf.Min(minY, points[i].y);
                }
                hasBounds = true;
                if (minY <= layerUpperY + .0001f)
                    return false;
            }
            return hasBounds;
        }

        private void QueueElevatedObjectTile(int x, int layerY, int z)
        {
            var viewSize = _super.data.mainData.viewSize;
            if (x < viewCenter.x - viewSize.x || x >= viewCenter.x + viewSize.x
                || z < viewCenter.z - viewSize.z || z >= viewCenter.z + viewSize.z)
                return;
            if (bfsVisited.Add((x, layerY, z)))
                bfsQueue.Enqueue((x, z));
        }

        private void CollectElevatedObjectVision(Vector3Int center)
        {
            // Same-layer connectivity is determined by real ownership, not
            // visual overlap. Only the elevated Objects hide, never their ground.
            bfsVisited.Add((center.x, center.y, center.z));
            bfsQueue.Enqueue((center.x, center.z));
            while (bfsQueue.Count > 0)
            {
                var cur = bfsQueue.Dequeue();
                var tile = _super.utilCtrl.GetTileData(cur.Item1, center.y, cur.Item2)?.unit;
                if (tile == null || !objectTileDic.TryGet(tile, out var objects))
                    continue;
                bool connects = false;
                foreach (var unit in objects)
                {
                    if (!objectTileDic.TryGetFirst(unit, out var owner) || owner != tile
                        || !IsObjectAboveTile(unit, tile))
                        continue;
                    connects = true;
                    nextVision[unit] = FullHighLayerOcclusionDegree;
                }
                if (!connects)
                    continue;
                QueueElevatedObjectTile(cur.Item1 - 1, center.y, cur.Item2);
                QueueElevatedObjectTile(cur.Item1 + 1, center.y, cur.Item2);
                QueueElevatedObjectTile(cur.Item1, center.y, cur.Item2 - 1);
                QueueElevatedObjectTile(cur.Item1, center.y, cur.Item2 + 1);
            }
        }

        private void CollectNearbyOcclusion(Vector3Int center, bool collectTiles)
        {
            float halfDepth = Mathf.Abs(_super.data.mainData.mapUnitSize.z) * .5f;
            // Reuse the visual footprint index: an elevated Object can belong to
            // a lower Tile, and missing intermediate layers must not stop the scan.
            for (int x = center.x - ObjectOcclusionHalfWidth; x <= center.x + ObjectOcclusionHalfWidth; x++)
            for (int depth = 0; depth <= OcclusionBackwardDistance; depth++)
            for (int height = 0; height <= OcclusionHeight; height++)
            {
                int z = center.z - depth;
                int y = center.y + height;
                var tileData = _super.utilCtrl.GetTileData(x, y, z);
                if (tileData == null)
                    continue;

                // Height >= depth is a fast hit, not the only hit: a lower Tile
                // can still project into the nearby fade region (e.g. depth 3 / height 2).
                // Test the projected position rather than the distant source Tile anchor.
                if (collectTiles && height > 0 && (height >= depth
                    || IsWithinHalfOcclusionRange(x, z + height, center)))
                {
                    CollectHighTileVision(tileData.unit, HalfHighLayerOcclusionDegree, center);
                    // Retain the existing connected-component behavior after a direct hit.
                    BfsLayerVision(y, x, z, HalfHighLayerOcclusionDegree, center);
                }

                if (objectTileDic.TryGet(tileData.unit, out var objects))
                    foreach (var unit in objects)
                    {
                        // A failed Tile boundary must not exclude another, lower boundary.
                        if (nearbyOccludingObjects.Contains(unit)
                            || !ProjectsForNearbyOcclusion(unit, tileData.unit, halfDepth, center))
                            continue;

                        nearbyOccludingObjects.Add(unit);
                        nextVision[unit] = CurrentLayerObjectOcclusionDegree;
                    }
            }
        }

        /// <summary>
        /// Manage vison
        /// </summary>
        private void UpdateVision()
        {
            // The shader uses the rendering camera's center, not each unit's anchor.
            // Share the existing three-cell X/Z range; camera zoom needs no MPB resubmission.
            Vector3 cellSize = _super.data.mainData.mapUnitSize;
            Shader.SetGlobalVector(FadeCenterRangeProperty, new Vector4(
                Mathf.Max(Mathf.Abs(cellSize.x) * HalfHighLayerOcclusionRadius, 0.0001f),
                Mathf.Max(Mathf.Abs(cellSize.z) * HalfHighLayerOcclusionRadius, 0.0001f), 0f, 0f));
            Shader.SetGlobalVector(FadeCenterPositionProperty, curCenterPos);
            nextVision.Clear();
            frontVisionOverrides.Clear();
            CollectVision();

            // A wide Object can stop occluding while its owner is outside the
            // current Tile set. Restore it if its instance is still displayed.
            foreach (var old in previousVision)
                if (!nextVision.ContainsKey(old.Key) && old.Key.isShowing)
                    ApplyUnitVision(old.Key, 1f);

            foreach (var current in nextVision)
            {
                if (current.Key is TileUnit tile
                    && frontVisionOverrides.TryGetValue(tile, out float frontDegree))
                    ApplyUnitVision(current.Key, current.Value, frontDegree);
                else
                    ApplyUnitVision(current.Key, current.Value);
            }

            var buffer = previousVision;
            previousVision = nextVision;
            nextVision = buffer;
            nextVision.Clear();
        }

        private void CollectVision()
        {
            var viewSize = _super.data.mainData.viewSize;
            var realViewCenter = _super.utilCtrl.RealPos2MapPosInt(curCenterPos);
            highLayerOcclusionObjects.Clear();
            objectProjectionRanges.Clear();
            objectHalfOcclusionEligibility.Clear();
            projectedUpperTileChecks.Clear();
            nearbyOccludingObjects.Clear();
            bfsVisited.Clear();
            bfsQueue.Clear();
            bool overlayHide = GlobalSettings.OVERLAY_HIDE;
            // 侧视模式：相机角度让前方(z更小方向)的高层会遮挡视线，需要扩展z检测范围
            bool isSideView = DynamicGlobalSettings.cameraMode == CameraMode.Isometric;
            bool modHighLayerHalfTransparent = GlobalSettings.MOD_HIGH_LAYER_HALF_TRANSPARENT
                && !DynamicGlobalSettings.playing;

            foreach (var curMap in curTileLst)
            {
                bool isHighLayer = curMap.mapPos.y > realViewCenter.y;
                nextVision[curMap.unit] = modHighLayerHalfTransparent && isHighLayer
                    ? HalfHighLayerOcclusionDegree
                    : 1f;
                if (modHighLayerHalfTransparent && isHighLayer)
                    CollectHighLayerObjectCandidates(curMap.unit, HalfHighLayerOcclusionDegree);
            }

            // Mod 编辑时高层统一半透明，不再应用 Play 的高层遮挡 BFS。
            if (!modHighLayerHalfTransparent)
            {
                // Keep the existing overhead/full-component rules; side-view half hits use the shared scan.
                int range = overlayHide ? 1 : 0;

                for (int i = realViewCenter.y + 1; i < realViewCenter.y + viewSize.y; i++)
                {
                    // 玩家正上方有可遮挡 Tile 时，该层的中心连通块全透，无需四邻格。
                    // 仍保留投影上层 Collider 的可见例外；其他连通块仍半透。
                    if (ShouldFullyOccludeHighLayer(i, realViewCenter))
                        BfsLayerVision(i, realViewCenter.x, realViewCenter.z,
                            FullHighLayerOcclusionDegree, realViewCenter);

                    if (isSideView)
                        continue; // Side-view Tile/Object candidates share the bounded scan below.

                    int startDz = -range;

                    for (int dx = -range; dx <= range; dx++)
                    {
                        for (int dz = startDz; dz <= range; dz++)
                        {
                            int checkX = realViewCenter.x + dx;
                            int checkZ = realViewCenter.z + dz;
                            BfsLayerVision(i, checkX, checkZ, HalfHighLayerOcclusionDegree, realViewCenter);
                        }
                    }
                }
            }

            // X +/-3, then Z 0..-5, then Y 0..+5; one Tile lookup feeds both kinds.
            if (isSideView)
                CollectNearbyOcclusion(realViewCenter, !modHighLayerHalfTransparent);
            CollectHighLayerObjectOcclusion(realViewCenter.y, isSideView, modHighLayerHalfTransparent);
            // A raised Object can still belong to this layer. Its center-connected
            // roof component overrides nearby/mixed half-opacity, but not Tile state.
            if (!modHighLayerHalfTransparent)
                CollectElevatedObjectVision(realViewCenter);
            CollectAttachedVision(realViewCenter.y, isSideView, modHighLayerHalfTransparent);
        }

        private void CollectAttachedVision(int currentLayer, bool isSideView, bool modHighLayerHalfTransparent)
        {
            // Tiles are final now. Visit displayed units once, not the three
            // reverse indexes for every Tile during both reset and BFS.
            foreach (var data in curObjectLst)
            {
                var unit = data.unit;
                // Visual-footprint occlusion overrides the owner's degree,
                // including Objects whose owner lies outside the current view.
                if (!nextVision.ContainsKey(unit))
                    CollectUnitVision(unit, objectTileDic);
                nextVision[unit] = GetObjectOcclusionDegree(unit, nextVision[unit], currentLayer, isSideView, modHighLayerHalfTransparent);
            }
            foreach (var data in curItemLst)
                CollectUnitVision(data.unit, itemTileDic);
            foreach (var data in curCharacterLst)
                CollectUnitVision(data.unit, characterTileDic);
        }

        private void CollectUnitVision<T>(T unit, DoubleDictionary<T, TileUnit> owners) where T : MapUnit
        {
            float degree = 1f;
            if (owners.TryGetFirst(unit, out var owner) && nextVision.TryGetValue(owner, out var tileDegree))
                degree = tileDegree;
            nextVision[unit] = degree;
        }


        public void RefreshCharacterOverlap(CharacterUnit unit)
        {
            if (unit == null)
                return;

            RefreshUnitColumns(unit);

            TileUnit owner = characterTileDic.GetFirst(unit);
            if (owner == null)
            {
                characterOverlapTileDic.Del(unit);
                return;
            }

            // This query has no callbacks, so one controller-owned scratch set is
            // safe. Always query the complete footprint, including upper layers.
            characterOverlapBuffer.Clear();
            characterOverlapBuffer.UnionWith(
                _super.utilCtrl.GetCharacterCollisionTiles(unit, Vector3.zero, CollideType.All));
            if (characterOverlapBuffer.Count == 0)
                characterOverlapBuffer.Add(owner);

            var oldTiles = characterOverlapTileDic.Get(unit);
            for (int i = oldTiles.Count - 1; i >= 0; i--)
                if (!characterOverlapBuffer.Contains(oldTiles[i]))
                    characterOverlapTileDic.Del(unit, oldTiles[i]);

            foreach (var tile in characterOverlapBuffer)
                if (!oldTiles.Contains(tile))
                    characterOverlapTileDic.Add(unit, tile);
            characterOverlapBuffer.Clear();
        }
        public void RefreshObjectOverlap(ObjectUnit unit)
        {
            if (unit == null)
                return;

            RefreshUnitColumns(unit);

            var affectedTiles = new HashSet<TileUnit>(objectTileDic.Get(unit));
            if (objectPassTypeTileDic.TryGet(unit, out var oldPassTiles))
                affectedTiles.UnionWith(oldPassTiles);
            if (objectNavigationTileDic.TryGet(unit, out var oldNavigationTiles))
                affectedTiles.UnionWith(oldNavigationTiles);
            objectTileDic.Del(unit);
            objectPassTypeTileDic.Del(unit);
            objectNavigationTileDic.Del(unit);
            foreach (var tile in _super.utilCtrl.GetVisionOverlap(unit.data))
            {
                objectTileDic.Add(unit, tile);
                affectedTiles.Add(tile);
            }
            foreach (var tile in _super.utilCtrl.GetObjectPassTypeOverlap(unit.data))
            {
                objectPassTypeTileDic.Add(unit, tile);
                affectedTiles.Add(tile);
            }
            foreach (var tile in _super.utilCtrl.GetObjectNavigationOverlap(unit.data))
            {
                objectNavigationTileDic.Add(unit, tile);
                affectedTiles.Add(tile);
            }
            RefreshObjectAffectedTiles(affectedTiles);
        }

        public void RemoveObjectOverlap(ObjectUnit unit)
        {
            ForgetUnitColumns(unit);
            var affectedTiles = new HashSet<TileUnit>(objectTileDic.Get(unit));
            if (objectPassTypeTileDic.TryGet(unit, out var oldPassTiles))
                affectedTiles.UnionWith(oldPassTiles);
            if (objectNavigationTileDic.TryGet(unit, out var oldNavigationTiles))
                affectedTiles.UnionWith(oldNavigationTiles);
            objectTileDic.Del(unit);
            objectPassTypeTileDic.Del(unit);
            objectNavigationTileDic.Del(unit);
            RefreshObjectAffectedTiles(affectedTiles);
        }

        private void RefreshObjectAffectedTiles(IEnumerable<TileUnit> tiles)
        {
            foreach (var tile in tiles)
            {
                bool covered = objectPassTypeTileDic.TryGet(tile, out var objects) && objects.Count > 0;
                tile.SetObjectCenterCovered(covered);
            }

            if (_super.navigationCtrl?.navUnits != null)
                _super.navigationCtrl.RefreshObjectTiles(tiles);
        }
        public Vector3 GetNavDir(Vector3 cur, Vector3 tar, int maxStep = 99999, float agentRadius = 0f,
            IReadOnlyCollection<int> passTypes = null, NavigationEndpointState endpointState = null)
        {
            return _super.navigationCtrl.GetNextDir(cur, tar, maxStep, agentRadius, passTypes, endpointState);
        }
        public void ResetView()
        {
            UpdateInfo(true);
        }
        public void UpdateInfo(bool forceFresh = false, bool simulateCharacters = true)
        {
            UpdateMapInfo(simulateCharacters);
            UpdateView(forceFresh);
        }

        // History/load restoration must not move Characters through navigation or
        // gravity while refreshing the newly restored instances in the same frame.
        public void RefreshView()
        {
            UpdateView(true);
        }

        private void UpdateView(bool forceFresh)
        {
            if (forceFresh || (curCenterPos - lastCenterPos).sqrMagnitude >= 1f
                || lastViewIsometric != (DynamicGlobalSettings.cameraMode == CameraMode.Isometric))
            {
                viewCenter = _super.utilCtrl.RealPos2MapPosInt(curCenterPos);

                if (GlobalSettings.MAP_SHOW_DEBUG)
                {
                    Z_Log.Log("pos:" + curCenterPos + " to now cam Pos:" + viewCenter);
                }
                FreshMap(forceFresh);
                lastCenterPos = curCenterPos;
            }
            UpdateVision();


        }
        public void SetGroupVision(TileUnit unit, float degree)
        {
            // Preserve the immediate public operation; per-frame collection
            // uses the tile-first path instead of repeatedly expanding groups.
            ApplyUnitVision(unit, degree);
            if (objectTileDic.TryGet(unit, out var objects))
                foreach (var obj in objects)
                    if (obj.belongTile == unit)
                        ApplyUnitVision(obj, degree);
            if (itemTileDic.TryGet(unit, out var items))
                foreach (var item in items)
                    if (item.belongTile == unit)
                        ApplyUnitVision(item, degree);
            if (characterTileDic.TryGet(unit, out var characters))
                foreach (var character in characters)
                    if (character.belongTile == unit)
                        ApplyUnitVision(character, degree);
        }

        private static void ApplyUnitVision(Unit unit, float degree, float? tileFrontDegree = null)
        {
            if (unit.ins == null)
                return;
            if (tileFrontDegree.HasValue && unit.ins is TileInstance tileInstance)
                tileInstance.ApplyVision(degree, tileFrontDegree.Value);
            else if (unit.ins is MapInstance instance)
                instance.ApplyVision(degree);
            else if (degree <= 0f)
                unit.ins.VisOff();
            else
            {
                unit.ins.VisOn();
                unit.ins.VisDegree(degree);
            }

            // Bound units may have been shown/replaced independently this frame.
            foreach (var child in unit.subUnits)
                ApplyUnitVision(child, degree);
        }
        /// <summary>
        /// 应用移动：更新单位位置、朝向、所属tile，并触发碰撞事件
        /// teleport=true时不触发碰撞检测（传送）
        /// </summary>
        public void ApplyMove(MapUnit unit, Vector3 newPos, Vector3 euler, bool teleport = false)
        {
            var movingCharacter = unit as CharacterUnit;
            if (movingCharacter != null && teleport)
                movingCharacter.ResetNavigationEndpoint();
            var oldPos = unit.data.pos;
            var oldEuler = unit.data.euler;
            var oldCharacterOverlap = movingCharacter == null
                ? null
                : new List<TileUnit>(characterOverlapTileDic.Get(movingCharacter));
            if (movingCharacter != null && !teleport)
            {
                newPos = _super.utilCtrl.ClampMoveToAreaBoundary(oldPos, newPos, movingCharacter.mapBoundaryDistance);
                // 先完成地图边界修正，再以最终中心所属 Tile 检查 passType。
                // 否则边界修正可能把已检查的位置再推入受限 Tile。
                if (!movingCharacter.isNavEndpointMove)
                    newPos = movingCharacter.ClampMoveToPassType(oldPos, newPos);
            }
            var newMapPos = _super.utilCtrl.RealPos2MapPosInt(newPos);
            if (!_super.utilCtrl.InArea(newMapPos))
            {
                //InArea已包含下方有tile的判断，此处为完全不在区域内，拉回最近有效位置
                newPos = movingCharacter == null
                    ? _super.utilCtrl.GetClosestInArea(newPos)
                    : _super.utilCtrl.GetClosestInArea(newPos,
                        movingCharacter.isNavEndpointMove ? null : movingCharacter.passTypes);
                newMapPos = _super.utilCtrl.RealPos2MapPosInt(newPos);
            }
            // 决定关联哪个tile：防止重力微移导致y截断后误切换到下方tile
            TileUnit newMap = null;
            /*  var floatMapPos = _super.utilCtrl.RealPos2MapPos(newPos);
                  // snap阈值需按unitSize.y缩放，保持真实空间容差固定为0.1（原unitSize=2时0.05 map空间=0.1真实空间）
                  if (Math.Abs(floatMapPos.y - newMapPos.y) < 0.1f / _super.data.mainData.mapUnitSize.y
                      && _super.utilCtrl.ContainsTile(newMapPos.x, newMapPos.y, newMapPos.z))
                  {
                      var tileData = _super.utilCtrl.GetTileData(newMapPos.x, newMapPos.y, newMapPos.z);
                   if (tileData != null&&tileData.prefabName == MapInfo.GetPrefabName("mapground"))
                              {
                                  newMap = tileData.unit;
                                  //仅在非爬升时吸附Y到地面tile高度，避免覆盖斜面滑行的+Y分量
                                  //重力开启时不吸附Y：球体碰撞体中心相对data.pos有偏移，吸附到tile高度会导致球体悬空，重力无法使球体落地
                                  float deltaY = newPos.y - unit.data.pos.y;
                                  if (deltaY <= 0.0001f && !GlobalSettings.ENABLE_GRAVITY)
                                  {
                                      newPos.y = newMap.data.pos.y;
                                  }
                               }
                 }*/

            // 策略3：以上都不满足，取下方最近的tile
            if (newMap == null)
            {
                newMap = _super.utilCtrl.GetTile(newMapPos.x, newMapPos.y, newMapPos.z);
            }
            if (!teleport && oldPos != newPos)
            {
                // 碰撞算法从当前Mesh位置沿dir扫掠，因此必须在写入newPos之前检测旧位置到新位置。
                CheckCollideEvent(unit, newPos - oldPos, oldCharacterOverlap);
            }
            if (newMap != null)
            {
                if (movingCharacter != null)
                    characterTileDic.Move(movingCharacter, newMap);
                else if (unit is ItemUnit item)
                    itemTileDic.Move(item, newMap);
            }

            if (unit.ins != null)
            {
                unit.ins.transform.position = newPos;
                unit.ins.transform.eulerAngles = euler;

            }
            unit.data.pos = newPos;
            unit.data.euler = euler;
            if (unit is ItemUnit) RefreshUnitColumns(unit);
            if (movingCharacter != null)
                RefreshCharacterOverlap(movingCharacter);
            else if (unit is ObjectUnit movingObject && (oldPos != newPos || oldEuler != euler))
                RefreshObjectOverlap(movingObject);
        }
        public void CheckCollideEvent(Unit unit, Vector3 dir, IEnumerable<TileUnit> extraTiles = null)
        {
            var candidateTiles = new HashSet<TileUnit>();
            if (unit is CharacterUnit ch)
            {
                if (extraTiles != null)
                    candidateTiles.UnionWith(extraTiles);
                candidateTiles.UnionWith(characterOverlapTileDic.Get(ch));
                candidateTiles.UnionWith(_super.utilCtrl.GetCharacterCollisionTiles(ch, dir, CollideType.All));
            }
            else
            {
                TileUnit cur = null;
                if (unit is ObjectUnit obj)
                    cur = objectTileDic.GetFirst(obj);
                else if (unit is ItemUnit item)
                    cur = itemTileDic.GetFirst(item);

                if (cur != null)
                    candidateTiles.UnionWith(_super.utilCtrl.GetNineTile((cur.data.mapPos.x, cur.data.mapPos.y, cur.data.mapPos.z), dir.magnitude));
            }

            HashSet<int> exist = new HashSet<int>() { unit.data.uid };
            foreach (var tile in candidateTiles)
            {
                // TileTouch表示移动单位的实体Collider与地面Tile接触，沿用Unit.OnEnter/OnExit的状态链路。
                CheckCollide((MapUnit)unit, tile, dir, CollideType.CollideOnly, out _, (target, res, dis) =>
                {
                    ManageTriggerEvent(unit, target, res);
                });

                var lst = new List<Unit>(objectTileDic.Get(tile));
                lst.AddRange(itemTileDic.Get(tile));
                lst.AddRange(characterOverlapTileDic.Get(tile));

                foreach (var tar in lst)
                {
                    if (exist.Contains(tar.data.uid))
                        continue;
                    exist.Add(tar.data.uid);
                    CheckCollide((MapUnit)unit, (MapUnit)tar, dir, CollideType.TriggerOnly, out _, (target, res, dis) =>
                    {
                        ManageTriggerEvent(unit, target, res);
                    });
                }
            }
        }
        private void ManageTriggerEvent(Unit a, Unit b, Graph.IntersectType type)
        {
            TimeManager.instance.AddCurLateUpdateWithoutCheckAction(() =>
            {
                switch (type)
                {
                    case Graph.IntersectType.In:
                        b.OnEnter(a);
                        a.OnEnter(b);
                        break;
                    case Graph.IntersectType.Out:
                        b.OnExit(a);
                        a.OnExit(b);
                        break;
                    case Graph.IntersectType.Cross:
                        b.OnEnter(a);
                        a.OnEnter(b);
                        b.OnExit(a);
                        a.OnExit(b);
                        break;
                }
            });

        }
        /// <summary>
        /// 碰撞检测调度（MapUnit级别）：遍历触发者的所有Mesh，对每个Mesh调用下层CheckCollide
        /// 返回最短碰撞距离disRes和对应的避障方向avoidDir
        /// </summary>
        public float CheckCollide(MapUnit trigger, MapUnit unit, Vector3 dir, CollideType type, out List<Vector3> avoidDir, Action<Unit, Graph.IntersectType, float> onCast = null)
        {
            avoidDir = new List<Vector3>();
            var disRes = (dir).magnitude;
            var assist = new Graph.IntersectAssisant(trigger.data.collidingUnitUid.Contains(unit.data.uid));

            foreach (var cur in trigger.GetMeshes(type))
            {
                float dis = CheckCollide(cur, unit, dir, type, out var avoidDirTmp, out var assistTmp);
                assist.Merge(assistTmp);
                if (MathF.Abs(dis) <= 0.01f && MathF.Abs(disRes) <= 0.01f)
                {
                    //已嵌入：智能合并避障方向（仅保留同半球兼容方向，避免冲突方向污染滑行）
                    MergeAvoidDirRange(avoidDir, avoidDirTmp);
                }
                else if (dis < disRes)
                {
                    disRes = dis;
                    avoidDir.Clear();
                    MergeAvoidDirRange(avoidDir, avoidDirTmp);
                }
            }
            var res = assist.GetRes();
            if (res != IntersectType.None)
            {
                onCast?.Invoke(unit, res, disRes);
            }
            return disRes;
        }
        /// <summary>
        /// 碰撞检测调度（MeshInfo级别）：遍历目标Unit的所有Mesh，调用MeshIntersectMesh进行SAT交叉检测
        /// 返回最短碰撞距离disRes和对应的避障法线方向avoidDir
        /// </summary>
        public float CheckCollide(MeshInfo trigger, MapUnit unit, Vector3 dir, CollideType type, out List<Vector3> avoidDir, out IntersectAssisant assist, Action<Unit, Graph.IntersectType, float> onCast = null)
        {
            float disRes = (dir).magnitude;
            avoidDir = new List<Vector3>();
            assist = new Graph.IntersectAssisant(false);
            foreach (var tar in unit.GetMeshes(type))
            {
                float dis = 0;
                //MeshIntersectMesh: 根据Mesh类型(Cube/Sphere)分发SAT碰撞检测
                var curType = Mesh.MeshIntersectMesh(trigger, tar, dir, out dis, out var avoid);

                assist.Add(curType);
                if (MathF.Abs(dis) <= 0.01f && MathF.Abs(disRes) <= 0.01f)
                {
                    //已嵌入：智能合并避障方向（仅保留同半球兼容方向，避免冲突方向污染滑行）
                    MergeAvoidDir(avoidDir, avoid);
                }
                else if (dis < disRes)
                {
                    disRes = dis;
                    avoidDir.Clear();
                    MergeAvoidDir(avoidDir, avoid);
                }
            }
            return disRes;
        }

        public void Begin()
        {

            hasView = false;
            viewCenter = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
            var failList = new List<int>();
            foreach (var itemData in ItemUnitForm.DataByUid.Values)
            {
                RefreshUnitColumns(itemData.unit);
                if (_super.data.CheckItemUnit(itemData))
                {
                    var mapPos = _super.utilCtrl.RealPos2MapPosInt(itemData.pos);
                    if (_super.utilCtrl.InArea(mapPos))
                    {
                        itemTileDic.Add(itemData.unit, _super.utilCtrl.GetTile(mapPos.x, mapPos.y, mapPos.z));
                    }
                }
                else
                {
                    failList.Add(itemData.uid);
                }

            }

            foreach (var objectData in ObjectUnitForm.DataByUid.Values)
            {
                if (_super.data.CheckObjectUnit(objectData))
                {
                    RefreshObjectOverlap(objectData.unit);
                }
                else
                {
                    failList.Add(objectData.uid);
                }
            }

            foreach (var characterData in CharacterUnitForm.DataByUid.Values)
            {
                if (_super.data.CheckCharacterUnit(characterData))
                {
                    var mapPos = _super.utilCtrl.RealPos2MapPosInt(characterData.pos);
                    if (_super.utilCtrl.InArea(mapPos))
                    {
                        characterTileDic.Add(characterData.unit, _super.utilCtrl.GetTile(mapPos.x, mapPos.y, mapPos.z));
                        RefreshCharacterOverlap(characterData.unit);
                    }
                }
                else
                {
                    failList.Add(characterData.uid);
                }
            }

            foreach (var id in failList)
            {
                UnitForm.RemoveData(id);
            }
        }
        public void End()
        {
            if (curTileLst != null)
                curTileLst.Clear();
            if (newMapLst != null)
                newMapLst.Clear();
            if (delMapLst != null)
                delMapLst.Clear();
            if (curCharacterLst != null)
                curCharacterLst.Clear();
            if (curObjectLst != null)
                curObjectLst.Clear();
            if (curItemLst != null)
                curItemLst.Clear();

            lastView = (0, 0, 0, 0, 0, 0);
            hasView = false;
            lastCenterPos = Vector3.one * -9999999;
            objectTileDic.Clear();
            foreach (var tile in objectPassTypeTileDic.GetDicT2().Keys)
                tile.SetObjectCenterCovered(false);
            objectPassTypeTileDic.Clear();
            objectNavigationTileDic.Clear();
            characterTileDic.Clear();
            characterOverlapTileDic.Clear();
            itemTileDic.Clear();
            unitColumns.Clear();
            previousVision.Clear();
            nextVision.Clear();
            frontVisionOverrides.Clear();
            nearbyOccludingObjects.Clear();
            highLayerOcclusionObjects.Clear();
            objectProjectionRanges.Clear();
            objectHalfOcclusionEligibility.Clear();
            projectedUpperTileChecks.Clear();
            bfsVisited.Clear();
            bfsQueue.Clear();
            characterOverlapBuffer.Clear();
        }
        public void DebugShow()
        {
            foreach (var map in curTileLst)
            {
                if ((map.unit.ins.transform.GetChild(0).position - map.pos).sqrMagnitude > 0.2)
                    Debug.Log((map.unit.ins.transform.position - map.pos).sqrMagnitude + "    ->  " + map.pos + " " + map.unit.ins.transform.position);
            }
        }
    }
}
