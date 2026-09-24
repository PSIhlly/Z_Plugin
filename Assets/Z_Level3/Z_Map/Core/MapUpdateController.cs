using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Z_Debug;
using Z_DesignStyle;
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
        public DoubleDictionary<CharacterUnit, TileUnit> characterTileDic = new DoubleDictionary<CharacterUnit, TileUnit>();
        public DoubleDictionary<CharacterUnit, TileUnit> characterOverlapTileDic = new DoubleDictionary<CharacterUnit, TileUnit>();
        public DoubleDictionary<ItemUnit, TileUnit> itemTileDic = new DoubleDictionary<ItemUnit, TileUnit>();
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

        private void FreshMap(bool forceFullScan)
        {
            var viewSize = _super.data.mainData.viewSize;
            var curView = (viewCenter.x - viewSize.x, viewCenter.x + viewSize.x, viewCenter.y - viewSize.y, viewCenter.y + viewSize.y, viewCenter.z - viewSize.z, viewCenter.z + viewSize.z);
            newMapLst.Clear();
            delMapLst.Clear();
            if (!forceFullScan && hasView && curView.Equals(lastView))
                return;

            (int, int, int, int, int, int) commonView = (Mathf.Max(curView.Item1, lastView.Item1), Mathf.Min(curView.Item2, lastView.Item2),
                Mathf.Max(curView.Item3, lastView.Item3), Mathf.Min(curView.Item4, lastView.Item4),
                Mathf.Max(curView.Item5, lastView.Item5), Mathf.Min(curView.Item6, lastView.Item6));

            // Remove only Tiles that actually left this view (or were replaced in-place).
            foreach (var map in curTileLst)
            {
                if (map.mapPos.x >= curView.Item2 || map.mapPos.x < curView.Item1
                    || map.mapPos.y >= curView.Item4 || map.mapPos.y < curView.Item3
                    || map.mapPos.z >= curView.Item6 || map.mapPos.z < curView.Item5
                    || !_super.data.maps.TryGetValue((map.mapPos.x, map.mapPos.y, map.mapPos.z), out var current)
                    || !ReferenceEquals(current, map))
                {
                    map.unit.Hide();
                    delMapLst.Add(map);
                }
            }
            curTileLst.ExceptWith(delMapLst);

            bool overlapsLastView = hasView
                && curView.Item1 < lastView.Item2 && curView.Item2 > lastView.Item1
                && curView.Item3 < lastView.Item4 && curView.Item4 > lastView.Item3
                && curView.Item5 < lastView.Item6 && curView.Item6 > lastView.Item5;

            // First/forced/non-overlapping refresh scans the current view once.
            // Ordinary movement scans only the non-overlapping entering slabs.
            if (forceFullScan || !overlapsLastView)
            {
                ShowAndAddLst(curTileLst, curView.Item1, curView.Item2, curView.Item3, curView.Item4, curView.Item5, curView.Item6);
            }
            else
            {
                if (curView.Item1 < lastView.Item1)
                    ShowAndAddLst(curTileLst, curView.Item1, Mathf.Min(lastView.Item1, curView.Item2), curView.Item3, curView.Item4, curView.Item5, curView.Item6);
                if (curView.Item2 > lastView.Item2)
                    ShowAndAddLst(curTileLst, Mathf.Max(lastView.Item2, curView.Item1), curView.Item2, curView.Item3, curView.Item4, curView.Item5, curView.Item6);

                if (curView.Item3 < lastView.Item3)
                    ShowAndAddLst(curTileLst, commonView.Item1, commonView.Item2, curView.Item3, Mathf.Min(lastView.Item3, curView.Item4), curView.Item5, curView.Item6);
                if (curView.Item4 > lastView.Item4)
                    ShowAndAddLst(curTileLst, commonView.Item1, commonView.Item2, Mathf.Max(lastView.Item4, curView.Item3), curView.Item4, curView.Item5, curView.Item6);

                if (curView.Item5 < lastView.Item5)
                    ShowAndAddLst(curTileLst, commonView.Item1, commonView.Item2, commonView.Item3, commonView.Item4, curView.Item5, Mathf.Min(lastView.Item5, curView.Item6));
                if (curView.Item6 > lastView.Item6)
                    ShowAndAddLst(curTileLst, commonView.Item1, commonView.Item2, commonView.Item3, commonView.Item4, Mathf.Max(lastView.Item6, curView.Item5), curView.Item6);
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
            for (int x = mapPos.x - 1; x <= mapPos.x + 1; x++)
            for (int z = mapPos.z - 1; z <= mapPos.z + 1; z++)
            {
                var cur = _super.data.maps.GetDv((x, mapPos.y, z), null);
                if (cur == null)
                    continue;

                cur.unit.Hide();
                curTileLst.Add(cur);
                cur.unit.Show();
                UpdateRelatedUnit(cur.unit);
            }
        }

        private void UpdateMapInfo()
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
                        obj.unit.UpdateInfo();
                    }
                    break;
                case GlobalSettings.UpdateType.All:
                    var lst = characterTileDic.GetDicT1().Keys.ToList();
                    foreach (var cUnit in lst)
                    {
                        cUnit.UpdateInfo();
                    }
                    break;
                case GlobalSettings.UpdateType.None:
                default:
                    break;
            }

        }

        private HashSet<(int, int)> bfsVisited = new HashSet<(int, int)>();
        private Queue<(int, int)> bfsQueue = new Queue<(int, int)>();
        private const float FullHighLayerOcclusionDegree = 0f;
        private const float HalfHighLayerOcclusionDegree = 0.5f;
        private const int HalfHighLayerOcclusionRadius = 3;
        private const int HalfHighLayerOcclusionRadiusSqr =
            HalfHighLayerOcclusionRadius * HalfHighLayerOcclusionRadius;
        private const float CurrentLayerObjectOcclusionDegree = 0.5f;
        private Dictionary<MapUnit, float> previousVision = new Dictionary<MapUnit, float>();
        private Dictionary<MapUnit, float> nextVision = new Dictionary<MapUnit, float>();
        private readonly Dictionary<TileUnit, float> frontVisionOverrides = new Dictionary<TileUnit, float>();
        private readonly HashSet<ObjectUnit> occlusionObjects = new HashSet<ObjectUnit>();
        private readonly Dictionary<ObjectUnit, float> highLayerOcclusionObjects = new Dictionary<ObjectUnit, float>();
        private readonly HashSet<TileUnit> characterOverlapBuffer = new HashSet<TileUnit>();

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
            var start = (centerX, centerZ);
            if (bfsVisited.Contains(start))
                return;
            bfsVisited.Add(start);
            bfsQueue.Enqueue(start);

            while (bfsQueue.Count > 0)
            {
                var cur = bfsQueue.Dequeue();
                var map = _super.utilCtrl.GetTileData(cur.Item1, layerY, cur.Item2);
                int offsetX = cur.Item1 - playerMapPos.x;
                int offsetZ = cur.Item2 - playerMapPos.z;
                bool withinHalfTransparentRange =
                    offsetX * offsetX + offsetZ * offsetZ <= HalfHighLayerOcclusionRadiusSqr;
                int projectedZ = cur.Item2 + layerY - playerMapPos.y;
                bool projectedTileWalkable = _super.navigationCtrl != null
                    && _super.navigationCtrl.IsBaseWalkable(cur.Item1, playerMapPos.y, projectedZ);
                if (!projectedTileWalkable)
                {
                    nextVision[map.unit] = 1f;
                }
                else if (degree == FullHighLayerOcclusionDegree || withinHalfTransparentRange)
                {
                    nextVision[map.unit] = degree;
                    // The authored front part projects one cell nearer to the player
                    // than the Tile body. Keep only renderer slots 3..5 opaque when
                    // that separate projected navigation cell is blocked.
                    bool frontProjectedTileWalkable = _super.navigationCtrl != null
                        && _super.navigationCtrl.IsBaseWalkable(
                            cur.Item1,
                            playerMapPos.y,
                            projectedZ - 1);
                    if (!frontProjectedTileWalkable)
                        frontVisionOverrides[map.unit] = 1f;
                    CollectHighLayerObjectCandidates(map.unit, degree);
                }

                for (int x = cur.Item1 - 1; x <= cur.Item1 + 1; x += 2)
                {
                    if (x < viewCenter.x - viewSize.x || x >= viewCenter.x + viewSize.x) continue;
                    if (!bfsVisited.Contains((x, cur.Item2)) && _super.utilCtrl.ContainsTile(x, layerY, cur.Item2))
                    {
                        bfsVisited.Add((x, cur.Item2));
                        bfsQueue.Enqueue((x, cur.Item2));
                    }
                }
                for (int z = cur.Item2 - 1; z <= cur.Item2 + 1; z += 2)
                {
                    if (z < viewCenter.z - viewSize.z || z >= viewCenter.z + viewSize.z) continue;
                    if (!bfsVisited.Contains((cur.Item1, z)) && _super.utilCtrl.ContainsTile(cur.Item1, layerY, z))
                    {
                        bfsVisited.Add((cur.Item1, z));
                        bfsQueue.Enqueue((cur.Item1, z));
                    }
                }
            }
        }

        private void CollectHighLayerObjectCandidates(TileUnit tile, float degree)
        {
            if (objectTileDic.TryGet(tile, out var objects))
                foreach (var unit in objects)
                    if (!highLayerOcclusionObjects.TryGetValue(unit, out var oldDegree) || degree < oldDegree)
                        highLayerOcclusionObjects[unit] = degree;
        }

        private void CollectHighLayerObjectOcclusion(int currentLayer)
        {
            foreach (var occlusion in highLayerOcclusionObjects)
            {
                var unit = occlusion.Key;
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
                nextVision[unit] = degree;
            }
        }

        private bool HasHighLayerOnFourSides(int layerY, Vector3Int center)
        {
            return _super.utilCtrl.ContainsTile(center.x - 1, layerY, center.z)
                && _super.utilCtrl.ContainsTile(center.x + 1, layerY, center.z)
                && _super.utilCtrl.ContainsTile(center.x, layerY, center.z - 1)
                && _super.utilCtrl.ContainsTile(center.x, layerY, center.z + 1);
        }

        private void UpdateCurrentLayerObjectOcclusion(Vector3Int center, bool isSideView)
        {
            occlusionObjects.Clear();
            if (!isSideView)
                return;

            int maxCellsBelow = _super.data.mainData.viewSize.y;
            for (int cellsBelow = 1; cellsBelow <= maxCellsBelow; cellsBelow++)
            {
                var tileData = _super.utilCtrl.GetTileData(
                    center.x,
                    center.y,
                    center.z - cellsBelow);
                if (tileData == null || !objectTileDic.TryGet(tileData.unit, out var objects))
                    continue;

                foreach (var unit in objects)
                {
                    if (!occlusionObjects.Add(unit)
                        || _super.utilCtrl.GetVisionHeightInTiles(unit.data, center.y) <= cellsBelow)
                        continue;

                    nextVision[unit] = CurrentLayerObjectOcclusionDegree;
                }
            }
        }

        /// <summary>
        /// Manage vison
        /// </summary>
        private void UpdateVision()
        {
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
                // 侧视模式按层高差向前扩展，并保留 1 格误差；命中后直接全透明。
                int range = overlayHide ? 1 : 0;

                for (int i = realViewCenter.y + 1; i < realViewCenter.y + viewSize.y; i++)
                {
                    bfsVisited.Clear();
                    bfsQueue.Clear();
                    int layerOffset = i - realViewCenter.y;
                    bool surroundedByHighLayer = HasHighLayerOnFourSides(i, realViewCenter);
                    float degree = surroundedByHighLayer
                        ? FullHighLayerOcclusionDegree
                        : HalfHighLayerOcclusionDegree;

                    // 四邻格都存在时从四边开始；玩家正上方的中心格允许为空。
                    if (surroundedByHighLayer)
                    {
                        BfsLayerVision(i, realViewCenter.x - 1, realViewCenter.z, degree, realViewCenter);
                        BfsLayerVision(i, realViewCenter.x + 1, realViewCenter.z, degree, realViewCenter);
                        BfsLayerVision(i, realViewCenter.x, realViewCenter.z - 1, degree, realViewCenter);
                        BfsLayerVision(i, realViewCenter.x, realViewCenter.z + 1, degree, realViewCenter);
                    }

                    // Isometric occlusion comes from smaller Z. Allow one extra cell
                    // beyond the layer gap as the fixed trigger tolerance.
                    int startDz = -range - (isSideView
                        ? layerOffset + 1
                        : 0);

                    for (int dx = -range; dx <= range; dx++)
                    {
                        for (int dz = startDz; dz <= range; dz++)
                        {
                            int checkX = realViewCenter.x + dx;
                            int checkZ = realViewCenter.z + dz;
                            BfsLayerVision(i, checkX, checkZ, degree, realViewCenter);
                        }
                    }
                }
            }

            CollectHighLayerObjectOcclusion(realViewCenter.y);
            // Apply this last so a current-layer occluder can never be overwritten as fully hidden.
            UpdateCurrentLayerObjectOcclusion(realViewCenter, isSideView);
            CollectAttachedVision();
        }

        private void CollectAttachedVision()
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

            objectTileDic.Del(unit);
            foreach (var tile in _super.utilCtrl.GetVisionOverlap(unit.data))
                objectTileDic.Add(unit, tile);
        }
        public Vector3 GetNavDir(Vector3 cur, Vector3 tar, int maxStep = 99999, float agentRadius = 0f,
            IReadOnlyCollection<int> passTypes = null)
        {
            return _super.navigationCtrl.GetNextDir(cur, tar, maxStep, agentRadius, passTypes);
        }
        public void ResetView()
        {
            UpdateInfo(true);
        }
        public void UpdateInfo(bool forceFresh = false)
        {
            UpdateMapInfo();
            if (forceFresh || (curCenterPos - lastCenterPos).sqrMagnitude >= 1f)
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
            var oldPos = unit.data.pos;
            var oldCharacterOverlap = movingCharacter == null
                ? null
                : new List<TileUnit>(characterOverlapTileDic.Get(movingCharacter));
            if (movingCharacter != null && !teleport)
            {
                newPos = _super.utilCtrl.ClampMoveToAreaBoundary(oldPos, newPos, movingCharacter.mapBoundaryDistance);
                // 先完成地图边界修正，再以最终中心所属 Tile 检查 passType。
                // 否则边界修正可能把已检查的位置再推入受限 Tile。
                newPos = movingCharacter.ClampMoveToPassType(oldPos, newPos);
            }
            var newMapPos = _super.utilCtrl.RealPos2MapPosInt(newPos);
            if (!_super.utilCtrl.InArea(newMapPos))
            {
                //InArea已包含下方有tile的判断，此处为完全不在区域内，拉回最近有效位置
                newPos = movingCharacter == null
                    ? _super.utilCtrl.GetClosestInArea(newPos)
                    : _super.utilCtrl.GetClosestInArea(newPos, movingCharacter.passTypes);
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
            if (movingCharacter != null)
                RefreshCharacterOverlap(movingCharacter);
            else if (unit is ObjectUnit movingObject)
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
            characterTileDic.Clear();
            characterOverlapTileDic.Clear();
            itemTileDic.Clear();
            previousVision.Clear();
            nextVision.Clear();
            frontVisionOverrides.Clear();
            occlusionObjects.Clear();
            highLayerOcclusionObjects.Clear();
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
