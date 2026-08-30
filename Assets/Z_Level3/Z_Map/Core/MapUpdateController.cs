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
        public List<TileUnitForm.Data> newMapLst
        {
            get;
            private set;
        }
        = new List<TileUnitForm.Data>();
        public List<TileUnitForm.Data> delMapLst
        {
            get;
            private set;
        }
        = new List<TileUnitForm.Data>();
        public List<ObjectUnitForm.Data> curObjectLst
        {
            get;
            private set;
        }
        = new List<ObjectUnitForm.Data>();
        public List<CharacterUnitForm.Data> curCharacterLst
        {
            get;
            private set;
        }
    = new List<CharacterUnitForm.Data>();
        public List<ItemUnitForm.Data> curItemLst
        {
            get;
            private set;
        }
       = new List<ItemUnitForm.Data>();
        public (int, int, int, int, int, int) lastView
        {
            get;
            private set;
        }

        private void FreshMap()
        {

            HashSet<TileUnitForm.Data> nowTmp = new HashSet<TileUnitForm.Data>();
            var viewSize = _super.data.mainData.viewSize;
            var curView = (viewCenter.x - viewSize.x, viewCenter.x + viewSize.x, viewCenter.y - viewSize.y, viewCenter.y + viewSize.y, viewCenter.z - viewSize.z, viewCenter.z + viewSize.z);
            (int, int, int, int, int, int) commonView = (Mathf.Max(curView.Item1, lastView.Item1), Mathf.Min(curView.Item2, lastView.Item2),
                Mathf.Max(curView.Item3, lastView.Item3), Mathf.Min(curView.Item4, lastView.Item4),
                Mathf.Max(curView.Item5, lastView.Item5), Mathf.Min(curView.Item6, lastView.Item6));

            //old:
            foreach (var map in curTileLst)
            {
                if (map.mapPos.x >= curView.Item2 || map.mapPos.x < curView.Item1
                    || map.mapPos.y >= curView.Item4 || map.mapPos.y < curView.Item3
                     || map.mapPos.z >= curView.Item6 || map.mapPos.z < curView.Item5)
                {
                    map.unit.Hide();
                }
                else
                {
                    nowTmp.Add(map);
                }
            }
            //fill
            if (curView.Item1 < lastView.Item1)
            {
                ShowAndAddLst(nowTmp, curView.Item1, Mathf.Min(lastView.Item1, curView.Item2), curView.Item3, curView.Item4, curView.Item5, curView.Item6);
            }
            else if (curView.Item2 > lastView.Item2)
            {
                ShowAndAddLst(nowTmp, Mathf.Max(lastView.Item2, curView.Item1), curView.Item2, curView.Item3, curView.Item4, curView.Item5, curView.Item6);
            }

            if (curView.Item3 < lastView.Item3)
            {
                ShowAndAddLst(nowTmp, commonView.Item1, commonView.Item2, curView.Item3, Mathf.Min(lastView.Item3, curView.Item4), curView.Item5, curView.Item6);
            }
            else if (curView.Item4 > lastView.Item4)
            {
                ShowAndAddLst(nowTmp, commonView.Item1, commonView.Item2, Mathf.Max(lastView.Item4, curView.Item3), curView.Item4, curView.Item5, curView.Item6);
            }

            if (curView.Item5 < lastView.Item5)
            {
                ShowAndAddLst(nowTmp, commonView.Item1, commonView.Item2, commonView.Item3, commonView.Item4, curView.Item5, Mathf.Min(lastView.Item5, curView.Item6));
            }
            else if (curView.Item6 > lastView.Item6)
            {
                ShowAndAddLst(nowTmp, commonView.Item1, commonView.Item2, commonView.Item3, commonView.Item4, Mathf.Max(lastView.Item6, curView.Item5), curView.Item6);
            }
            newMapLst.Clear();
            foreach (var now in nowTmp)
            {
                newMapLst.Add(now);
            }
            foreach (var old in curTileLst)
            {
                newMapLst.Remove(old);
                delMapLst.Add(old);
            }
            foreach (var now in nowTmp)
            {
                delMapLst.Remove(now);
            }

            foreach (var newMap in newMapLst)
            {
                UpdateRelatedUnit(newMap.unit);

            }
            foreach (var delMap in delMapLst)
            {
                UpdateRelatedUnit(delMap.unit);

            }

            curTileLst = nowTmp;
            lastView = curView;
        }
        private void UpdateRelatedUnit(TileUnit tile)
        {
            bool show = tile.isShowing;
            foreach (var ch in characterTileDic.Get(tile))
            {
                if (show)
                {
                    ch.Show();
                    if (!curCharacterLst.Contains(ch.data))
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
                if (show)
                {
                    item.Show();
                    if (!curItemLst.Contains(item.data))
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
                if (show)
                {
                    obj.Show();
                    if (!curObjectLst.Contains(obj.data))
                        curObjectLst.Add(obj.data);
                }
                else
                {
                    obj.Hide();
                    curObjectLst.Remove(obj.data);
                }
            }

        }
        private void ShowAndAddLst(HashSet<TileUnitForm.Data> lst, int minX, int maxX, int minY, int maxY, int minZ, int maxZ)
        {
            for (int i = minX; i < maxX; i++)
            {

                for (int k = minZ; k < maxZ; k++)
                {
                    if (!_super.data.mapXZ2Y.ContainsKey((i, k)))
                        continue;
                    int yMax = Mathf.Min(_super.data.mapXZ2Y[(i, k)].Max + 1, maxY);
                    int yMin = Mathf.Max(_super.data.mapXZ2Y[(i, k)].Min, minY);
                    for (int j = yMin; j < yMax; j++)
                    {
                        if (!_super.utilCtrl.InArea((i, j, k)))
                            continue;
                        var map = _super.utilCtrl.GetTileData(i, j, k);
                        map.unit.Show();
                        lst.Add(map);

                    }
                }
            }


        }
        public void UpdateSingleOne(MapUnit unit)
        {
            if (unit is TileUnit tl)
            {
                for(int i=tl.data.mapPos.x-1;i<= tl.data.mapPos.x +1;i++)
                for(int j=tl.data.mapPos.z-1;j<= tl.data.mapPos.z +1;j++)
                    {
                        var cur = _super.data.maps.GetDv((i, tl.data.mapPos.y, j), null);
                        if(cur!=null)
                        {
                            cur.unit.Hide();
                            if(!curTileLst.Contains(cur))
                            {
                                curTileLst.Add(cur);
                            }
                            cur.unit.Show();
                            UpdateRelatedUnit(cur.unit);
                        }
                    }
                      
            }
            else if (unit is CharacterUnit ch)
            {
                foreach (var tile in characterTileDic.Get(ch))
                {
                    if (tile.isShowing)
                    {
                        ch.Show();
                        if (!curCharacterLst.Contains(ch.data))
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
                        if (!curItemLst.Contains(it.data))
                            curItemLst.Add(it.data);
                    }
                }
            }
            else if (unit is ObjectUnit ob)
            {
                foreach (var tile in objectTileDic.Get(ob))
                {
                    if (tile.isShowing)
                    {
                        ob.Show();
                        if (!curObjectLst.Contains(ob.data))
                            curObjectLst.Add(ob.data);
                    }
                }
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
        private List<((int, int) pos, float sqrDist)> bfsStartLst = new List<((int, int), float)>();
        private static Comparison<((int, int) pos, float sqrDist)> bfsDistCompare =
            (a, b) => a.sqrDist.CompareTo(b.sqrDist);
        private const float OcclusionDegree = 0.5f;

        /// <summary>
        /// BFS遍历指定y层的tile，从(centerX,centerZ)开始，对遮挡层有tile的位置设置透明度
        /// 如果起始位置不存在tile则直接返回
        /// </summary>
        private void BfsLayerVision(int layerY, int centerX, int centerZ, float degree)
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
                SetGroupVision(map.unit, degree);

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

        private void UpdateCurrentLayerObjectOcclusion(Vector3Int center, bool isSideView)
        {
            if (!isSideView)
                return;

            var handled = new HashSet<ObjectUnit>();
            int maxCellsBelow = _super.data.mainData.viewSize.y;
            for (int cellsBelow = 1; cellsBelow <= maxCellsBelow; cellsBelow++)
            {
                var tileData = _super.utilCtrl.GetTileData(
                    center.x,
                    center.y,
                    center.z - cellsBelow);
                if (tileData == null)
                    continue;

                foreach (var unit in objectTileDic.Get(tileData.unit))
                {
                    if (!handled.Add(unit)
                        || _super.utilCtrl.GetVisionHeightInTiles(unit.data, center.y) <= cellsBelow)
                        continue;

                    unit.VisOn();
                    unit.VisDegree(OcclusionDegree);
                }
            }
        }

        /// <summary>
        /// Manage vison
        /// </summary>
        private void UpdateVision()
        {
            var viewSize = _super.data.mainData.viewSize;
            var realViewCenter = _super.utilCtrl.RealPos2MapPosInt(curCenterPos);
            bool overlayHide = GlobalSettings.OVERLAY_HIDE;
            // 侧视模式：相机角度让前方(z更小方向)的高层会遮挡视线，需要扩展z检测范围
            bool isSideView = DynamicGlobalSettings.cameraMode == CameraMode.Isometric;

            foreach (var curMap in curTileLst)
            {
                if (curMap.mapPos.y < realViewCenter.y)
                {

                    SetGroupVision(curMap.unit, 1);
                }
                else
                {
                    SetGroupVision(curMap.unit, 1);
                }
            }

            // 处理高层tile的遮挡：按距离从近到远，从各起始点BFS相连的高层
            int range = overlayHide ? 1 : 0;
            // 侧视模式下，dz范围扩展到-viewSize.y（角色前方z更小的方向），与OVERLAY_HIDE与否都生效
            int startDz = -range - (isSideView ? viewSize.y : 0);

            for (int i = realViewCenter.y + 1; i < realViewCenter.y + viewSize.y; i++)
            {
                bfsVisited.Clear();
                bfsQueue.Clear();
                bfsStartLst.Clear();

                for (int dx = -range; dx <= range; dx++)
                {
                    for (int dz = startDz; dz <= range; dz++)
                    {
                        int checkX = realViewCenter.x + dx;
                        int checkZ = realViewCenter.z + dz;
                        float dxN = curCenterPos.x - checkX;
                        float dzN = curCenterPos.z - checkZ;
                        bfsStartLst.Add(((checkX, checkZ), dxN * dxN + dzN * dzN));
                    }
                }
                bfsStartLst.Sort(bfsDistCompare);

                foreach (var d in bfsStartLst)
                {
                    var pos = d.pos;
                    float sqrDist = d.sqrDist;
                    float degree = Mathf.Max(
                        OcclusionDegree,
                        Math.Clamp(Mathf.Sqrt(sqrDist) - 0.75f, 0, 1));

                    // OVERLAY_HIDE额外逻辑：靠近的tile，其相连的高层也消失
                    bool nearHide = false;
                    if (overlayHide && sqrDist < 0.25f) // 0.5f^2
                    {
                        for (int yOffset = 0; yOffset <= 2; yOffset++)
                        {
                            int baseY = realViewCenter.y - yOffset;
                            if (baseY < 0) continue;
                            if (_super.utilCtrl.ContainsTile(pos.Item1, baseY, pos.Item2))
                            {
                                nearHide = true;
                                break;
                            }
                        }
                    }

                    BfsLayerVision(i, pos.Item1, pos.Item2, nearHide ? OcclusionDegree : degree);
                }
            }

            UpdateCurrentLayerObjectOcclusion(realViewCenter, isSideView);
        }


        public void RefreshCharacterOverlap(CharacterUnit unit)
        {
            if (unit == null)
                return;

            TileUnit owner = characterTileDic.GetFirst(unit);
            characterOverlapTileDic.Del(unit);
            if (owner == null)
                return;

            foreach (var tile in _super.utilCtrl.GetCharacterCollisionTiles(unit, Vector3.zero, CollideType.All))
            {
                if (!characterOverlapTileDic.Contains(unit, tile))
                    characterOverlapTileDic.Add(unit, tile);
            }

            if (characterOverlapTileDic.Get(unit).Count == 0)
                characterOverlapTileDic.Add(unit, owner);
        }
        public void RefreshObjectOverlap(ObjectUnit unit)
        {
            if (unit == null)
                return;

            objectTileDic.Del(unit);
            foreach (var tile in _super.utilCtrl.GetVisionOverlap(unit.data))
                objectTileDic.Add(unit, tile);
        }
        public Vector3 GetNavDir(Vector3 cur, Vector3 tar, int maxStep = 99999, float agentRadius = 0f)
        {
            return _super.navigationCtrl.GetNextDir(cur, tar, maxStep, agentRadius);
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
                FreshMap();
                lastCenterPos = curCenterPos;
            }
            UpdateVision();


        }
        public void SetGroupVision(TileUnit unit, float degree)
        {
            if (degree <= 0)
            {
                unit.VisOff();
                foreach (var curObj in objectTileDic.Get(unit))
                {
                    if (curObj.belongTile == unit)
                        curObj.VisOff();
                }
                foreach (var curItem in itemTileDic.Get(unit))
                {
                    if (curItem.belongTile == unit)
                        curItem.VisOff();
                }
                foreach (var curCh in characterTileDic.Get(unit))
                {
                    if (curCh.belongTile == unit)
                        curCh.VisOff();
                }
            }
            else if (degree >= 1)
            {

                unit.VisOn();
                unit.VisDegree(1f);
                foreach (var curObj in objectTileDic.Get(unit))
                {

                    if (curObj.belongTile == unit)
                    {
                        curObj.VisOn();
                        curObj.VisDegree(1f);
                    }
                }
                foreach (var curItem in itemTileDic.Get(unit))
                {
                    if (curItem.belongTile == unit)
                    {

                        curItem.VisOn();
                        curItem.VisDegree(1f);
                    }
                }
                foreach (var curCh in characterTileDic.Get(unit))
                {
                    if (curCh.belongTile == unit)
                    {
                        curCh.VisOn();
                        curCh.VisDegree(1f);
                    }
                }
            }
            else
            {
                unit.VisDegree(degree);
                foreach (var curObj in objectTileDic.Get(unit))
                {
                    if (curObj.belongTile == unit)
                    {
                        curObj.VisDegree(degree);
                    }
                }
                foreach (var curItem in itemTileDic.Get(unit))
                {
                    if (curItem.belongTile == unit)
                    {
                        curItem.VisDegree(degree);
                    }
                }
                foreach (var curCh in characterTileDic.Get(unit))
                {
                    if (curCh.belongTile == unit)
                    {
                        curCh.VisDegree(degree);
                    }
                }
            }

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
            }
            var newMapPos = _super.utilCtrl.RealPos2MapPosInt(newPos);
            if (!_super.utilCtrl.InArea(newMapPos))
            {
                //InArea已包含下方有tile的判断，此处为完全不在区域内，拉回最近有效位置
                newPos = _super.utilCtrl.GetClosestInArea(newPos);
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
            lastCenterPos = Vector3.one * -9999999;
            objectTileDic.Clear();
            characterTileDic.Clear();
            characterOverlapTileDic.Clear();
            itemTileDic.Clear();
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
