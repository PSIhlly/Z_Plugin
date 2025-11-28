using System;
using System.Collections;
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
using static UnityEditor.PlayerSettings;
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
        public DoubleDictionary<ItemUnit, TileUnit> itemTileDic = new DoubleDictionary<ItemUnit, TileUnit>();

        public List<TileUnitForm.Data> curMapLst
        {
            get;
            private set;
        }
        = new List<TileUnitForm.Data>();
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
        (int, int, int, int, int, int) lastView;

        private void FreshMap()
        {

            List<TileUnitForm.Data> nowTmp = new List<TileUnitForm.Data>();
            var viewSize = _super.data.mainData.viewSize;
            var curView = (viewCenter.x - viewSize.x, viewCenter.x + viewSize.x, viewCenter.y - viewSize.y, viewCenter.y + viewSize.y, viewCenter.z - viewSize.z, viewCenter.z + viewSize.z);
            (int, int, int, int, int, int) commonView = (Mathf.Max(curView.Item1, lastView.Item1), Mathf.Min(curView.Item2, lastView.Item2),
                Mathf.Max(curView.Item3, lastView.Item3), Mathf.Min(curView.Item4, lastView.Item4),
                Mathf.Max(curView.Item5, lastView.Item5), Mathf.Min(curView.Item6, lastView.Item6));

            //old:
            foreach (var map in curMapLst)
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
            foreach (var old in curMapLst)
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
                foreach (var ch in characterTileDic.Get(newMap.unit))
                {
                    ch.Show();
                    if (!curCharacterLst.Contains(ch.data))
                        curCharacterLst.Add(ch.data);
                }
                foreach (var ch in itemTileDic.Get(newMap.unit))
                {
                    ch.Show();
                    if (!curItemLst.Contains(ch.data))
                        curItemLst.Add(ch.data);
                }

                foreach (var ch in objectTileDic.Get(newMap.unit))
                {
                    ch.Show();
                    if (!curObjectLst.Contains(ch.data))
                        curObjectLst.Add(ch.data);
                }
            }
            foreach (var delMap in delMapLst)
            {
                foreach (var ch in itemTileDic.Get(delMap.unit))
                {
                    ch.Hide();
                    curItemLst.Remove(ch.data);
                }
                foreach (var ch in characterTileDic.Get(delMap.unit))
                {
                    ch.Hide();
                    curCharacterLst.Remove(ch.data);
                }
                foreach (var ch in objectTileDic.Get(delMap.unit))
                {
                    ch.Hide();
                    curObjectLst.Remove(ch.data);
                }
            }

            curMapLst = nowTmp;
            lastView = curView;
        }
        private void ShowAndAddLst(List<TileUnitForm.Data> lst, int minX, int maxX, int minY, int maxY, int minZ, int maxZ)
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
                        var map = _super.data.maps[(i, j, k)];
                        map.unit.Show();
                        lst.Add(map);

                    }
                }
            }


        }


        private void UpdateMapInfo()
        {
            //return;
            //update
            if (GlobalSettings.UPDATE_TILE_ALWAYS)
            {
                foreach (var map in curMapLst)
                {
                    map.unit.UpdateInfo();
                }
            }
            foreach (var map in curObjectLst)
            {
                map.unit.UpdateInfo();
            }
            foreach (var map in curItemLst)
            {
                map.unit.UpdateInfo();
            }
            foreach (var map in curCharacterLst)
            {
                map.unit.UpdateInfo();
            }


        }
        (int, int)[] dir9 = new[] {
                    (0, 0), (1, 0), (-1, 0),
                    (0, 1), (0, -1), (1, 1),
                    (-1,-1),(1, -1),(-1, 1)
                };
        /// <summary>
        /// Manage vison
        /// </summary>
        private void UpdateVision()
        {
            if (GlobalSettings.OVERLAY_HIDE)
            {
                var viewSize = _super.data.mainData.viewSize;
                foreach (var curMap in curMapLst)
                {
                    SetGroupVision(curMap.unit, 1);
                }

                HashSet<(int, int)> visited = new HashSet<(int, int)>();
                Queue<(int, int)> queue = new Queue<(int, int)>();
                for (int i = viewCenter.y + 1; i < viewCenter.y + viewSize.y; i++)
                {
                    visited.Clear();
                    queue.Clear();

                    List<((int, int), float)> dis = new List<((int, int), float)>();
                    for (int dirId = 0; dirId < dir9.Length; dirId++)
                    {
                        var dir = dir9[dirId];
                        dir.Item1 += viewCenter.x;
                        dir.Item2 += viewCenter.z;
                        dis.Add((dir, new Vector2(curCenterPos.x - dir.Item1, curCenterPos.z - dir.Item2).magnitude));
                    }
                    dis.Sort((a, b) => { return a.Item2.CompareTo(b.Item2); });

                    foreach (var d in dis)
                    {
                        var dir = d.Item1;
                        float degree = Math.Clamp(d.Item2 - 0.75f, 0, 1);
                        if (_super.data.maps.ContainsKey((dir.Item1, i, dir.Item2)))
                        {
                            if (!visited.Contains(dir))
                            {
                                visited.Add(dir);
                                queue.Enqueue(dir);
                            }
                            while (queue.Count > 0)
                            {
                                var cur = queue.Dequeue();
                                var map = _super.data.maps[(cur.Item1, i, cur.Item2)];
                                if (d.Item2 <= 0)
                                {
                                    SetGroupVision(map.unit, 0);

                                }
                                else
                                {
                                    SetGroupVision(map.unit, degree);

                                }

                                for (int x = cur.Item1 - 1; x <= cur.Item1 + 1 && x < dir.Item1 + viewSize.x && x >= dir.Item1 - viewSize.x; x += 2)
                                {
                                    if (!visited.Contains((x, cur.Item2)) && _super.data.maps.ContainsKey((x, i, cur.Item2)))
                                    {
                                        visited.Add((x, cur.Item2));
                                        queue.Enqueue((x, cur.Item2));
                                    }
                                }
                                for (int z = cur.Item2 - 1; z <= cur.Item2 + 1 && z < dir.Item2 + viewSize.z && z >= dir.Item2 - viewSize.z; z += 2)
                                {
                                    if (!visited.Contains((cur.Item1, z)) && _super.data.maps.ContainsKey((cur.Item1, i, z)))
                                    {
                                        visited.Add((cur.Item1, z));
                                        queue.Enqueue((cur.Item1, z));
                                    }
                                }

                            }

                        }

                    }

                }

            }
            else
            {
                foreach (var curMap in curMapLst)
                {
                    if (curMap.mapPos.y <= viewCenter.y)
                    {
                        curMap.unit.VisOn();
                    }
                    else
                    {
                        curMap.unit.VisOff();
                    }
                }
            }


        }


        public Vector3 GetNavDir(Vector3 cur, Vector3 tar, int maxStep = 99999)
        {
            return _super.navigationCtrl.GetNextDir(cur, tar, maxStep);
        }
        public void ResetInfo()
        {
            lastView = (0, 0, 0, 0, 0, 0);
            foreach (var map in curMapLst)
            {
                map.unit.Hide();
            }
            curMapLst.Clear();
            UpdateInfo(true);
        }
        public void UpdateInfo(bool forceFresh = false)
        {
            UpdateMapInfo();
            if (forceFresh || (curCenterPos - lastCenterPos).sqrMagnitude >= 1f)
            {
                viewCenter = _super.utilCtrl.RealPos2MapPos(curCenterPos);

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
                    curObj.VisOff();
                }
                foreach (var curItem in itemTileDic.Get(unit))
                {
                    curItem.VisOff();
                }
                foreach (var curCh in characterTileDic.Get(unit))
                {
                    curCh.VisOff();
                }
            }
            else if (degree >= 1)
            {

                unit.VisOn();
                unit.VisDegree(1f);
                foreach (var curObj in objectTileDic.Get(unit))
                {
                    curObj.VisOn();
                    curObj.VisDegree(1f);
                }
                foreach (var curItem in itemTileDic.Get(unit))
                {
                    curItem.VisOn();
                    curItem.VisDegree(1f);
                }
                foreach (var curCh in characterTileDic.Get(unit))
                {
                    curCh.VisOn();
                    curCh.VisDegree(1f);
                }
            }
            else
            {
                unit.VisDegree(degree);
                foreach (var curObj in objectTileDic.Get(unit))
                {
                    curObj.VisDegree(degree);
                }
                foreach (var curItem in itemTileDic.Get(unit))
                {
                    curItem.VisDegree(degree);
                }
                foreach (var curCh in characterTileDic.Get(unit))
                {
                    curCh.VisDegree(degree);
                }
            }

        }
        public void ApplyMove(Unit unit, Vector3 newPos, Vector3 euler)
        {
            var newMapPos = _super.utilCtrl.RealPos2MapPos(newPos);
            if (!_super.utilCtrl.InArea(newMapPos))
            {
                newPos = _super.utilCtrl.GetClosestInArea(newPos);
                newMapPos = _super.utilCtrl.RealPos2MapPos(newPos);
            }
            if (_super.data.maps.ContainsKey((newMapPos.x, newMapPos.y, newMapPos.z)))
            {
                var newMap = _super.data.maps[(newMapPos.x, newMapPos.y, newMapPos.z)].unit;

                if (unit is CharacterUnit ch)
                {
                    characterTileDic.Move(ch, newMap);
                }
                else if (unit is ObjectUnit obj)
                {
                    objectTileDic.Move(obj, newMap);
                }
                else if (unit is ItemUnit item)
                {
                    itemTileDic.Move(item, newMap);
                }


            }
            if (unit.ins != null)
            {
                unit.ins.transform.position = newPos;
            }
            var oldPos = unit.data.pos;
            unit.data.pos = newPos;
            unit.data.euler = euler;
               if(oldPos!= newPos)
            {
                MapManager.instance.updateCtrl.ChechCollideEvent(unit, Vector3.zero);
            }
        }
        public void ChechCollideEvent(Unit unit, Vector3 dir)
        {
            if (unit is CharacterUnit ch)
            {
                var cur = characterTileDic.Get(ch)[0];
                HashSet<int> exist = new HashSet<int>();
                foreach (var tile in _super.utilCtrl.GetNineTile((cur.data.mapPos.x, cur.data.mapPos.y, cur.data.mapPos.z)))
                {
                    foreach (var obj in objectTileDic.Get(tile))
                    {
                        if (exist.Contains(obj.data.uid))
                            continue;
                        exist.Add(obj.data.uid);

                        CheckCollide(ch, obj, dir, CollideType.TriggerOnly, (tar, res, dis) =>
                        {
                            ManageTriggerEvent(ch, tar, res);
                        });
                    }
                    foreach (var item in itemTileDic.Get(tile))
                    {
                        if (exist.Contains(item.data.uid))
                            continue;
                        exist.Add(item.data.uid);
                        CheckCollide(ch, item, dir, CollideType.TriggerOnly, (tar, res, dis) =>
                        {
                            ManageTriggerEvent(ch, tar, res);
                        });
                    }
                }
            }
        }
        private void ManageTriggerEvent(Unit a,Unit b, Graph.IntersectType type)
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
        public float CheckCollide(MapUnit trigger, MapUnit unit, Vector3 dir, CollideType type, Action<Unit, Graph.IntersectType, float> onCast = null)
        {
            var disRes = (dir).magnitude;
            var assist = new Graph.IntersectAssisant(trigger.data.collidingUnitUid.Contains(unit.data.uid));
            foreach (var tar in _super.utilCtrl.GetCollidersMesh(unit.prefab, unit.data.pos, unit.data.euler, unit.data.scale, type))
            {
                foreach (var cur in _super.utilCtrl.GetCollidersMesh(trigger.prefab, trigger.data.pos, trigger.data.euler, trigger.data.scale, type))
                {
                    float dis = 0;
                    var curType = Mesh.MeshIntersectMesh(cur, tar, dir, out dis);
                    assist.Add(curType);
                    disRes = Math.Min(disRes, dis);
                }
            }
            var res = assist.GetRes();
            if (res != IntersectType.None)
            {
                onCast?.Invoke(unit, res, disRes);
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
                    var mapPos = _super.utilCtrl.RealPos2MapPos(itemData.pos);
                    if (_super.utilCtrl.InArea(mapPos))
                    {
                        itemTileDic.Add(itemData.unit, _super.data.maps[(mapPos.x, mapPos.y, mapPos.z)].unit);
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
                    foreach (var m in _super.utilCtrl.GetOverlap(objectData))
                    {
                        objectTileDic.Add(objectData.unit, m);
                    }
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
                    var mapPos = _super.utilCtrl.RealPos2MapPos(characterData.pos);
                    if (_super.utilCtrl.InArea(mapPos))
                    {
                        characterTileDic.Add(characterData.unit, _super.data.maps[(mapPos.x, mapPos.y, mapPos.z)].unit);
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
            if (curMapLst != null)
                curMapLst.Clear();
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
            itemTileDic.Clear();
        }
        public void DebugShow()
        {
            foreach (var map in curMapLst)
            {
                if ((map.unit.ins.transform.GetChild(0).position - map.pos).sqrMagnitude > 0.2)
                    Debug.Log((map.unit.ins.transform.position - map.pos).sqrMagnitude + "    ->  " + map.pos + " " + map.unit.ins.transform.position);
            }
        }
    }
}
