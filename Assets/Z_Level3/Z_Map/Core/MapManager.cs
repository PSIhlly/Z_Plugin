using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Debug;
using Z_DesignStyle;
using Z_Map.Analysis;
using Z_Map.Form;
using Z_UnitSystem;
using Z_UnitSystem.Form;

namespace Z_Map
{
    public static class GlobalSettings
    {
        public const bool NAV_DEBUG = true;
        public const bool MAP_SHOW_DEBUG = false;
    }

    public class MapManager : Z_MonoManager<MapManager>
    {
        public MapDataController dataCtrl;
        public GameObject mainGo;
        

        public NavigationController navigationCtrl;
        public MapUtilController mapUtilCtrl;

        private Vector3Int curCenterPos;
        private Vector3Int viewCenter;

        private List<MapUnitForm.Data> curMapLst = new List<MapUnitForm.Data>();

        public override void Init()
        {
            base.Init();

            navigationCtrl = new NavigationController();
            navigationCtrl.Init(this);

            mapUtilCtrl = new MapUtilController();
            mapUtilCtrl.Init(this);
            
        }

        #region external

        public void Begin(MapDataController dataCtrl)
        {
            End();
            Init();
            mainGo.SetActive(true);
            this.dataCtrl = dataCtrl;
            

            viewCenter = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);

            foreach (var itemData in ItemUnitForm.DataByUid.Values)
            {
                var mapPos = mapUtilCtrl.RealPos2MapPos(itemData.pos);
                if (mapUtilCtrl.InArea(mapPos))
                {
                    MapUnitForm.DataByUid[dataCtrl.maps[mapPos.x, mapPos.y, mapPos.z].uid].unit.Bind(itemData.unit);
                }
            }
            foreach (var characterData in CharacterUnitForm.DataByUid.Values)
            {
                var mapPos = mapUtilCtrl.RealPos2MapPos(characterData.pos);
                if (mapUtilCtrl.InArea(mapPos))
                {
                    MapUnitForm.DataByUid[dataCtrl.maps[mapPos.x, mapPos.y, mapPos.z].uid].unit.Bind(characterData.unit);
                }
            }
            navigationCtrl.Build();
            mainGo.SetActive(true);
        }
        public void AddUnit(Unit unit)
        {
            if(unit is ItemUnit item)
            {
                ItemUnitForm.AddData(item.data);
            }else if(unit is CharacterUnit character)
            {
                CharacterUnitForm.AddData(character.data);
            }
            else if (unit is MapUnit map)
            {
                MapUnitForm.AddData(map.data);
            }

            unit.SubUpdateActive();
        }
        public void RemoveUnit(int uid)
        {
            var data = UnitForm.DataByUid[uid];
            var unit = data.unit;
            if (data is MapUnitForm.Data)
            {
                MapUnitForm.RemoveData(uid);
            }
            if (data is ItemUnitForm.Data)
            {
                ItemUnitForm.RemoveData(uid);
            }
            if (data is CharacterUnitForm.Data)
            {
                CharacterUnitForm.RemoveData(uid);
            }

            if (unit.superUnit != null)
                unit.superUnit.Unbind(data.unit);
            unit.VisOff();
            unit.Hide();
        }
        public void SetPos(Vector3 curCenterPos)
        {
            this.curCenterPos = mapUtilCtrl.RealPos2MapPos(curCenterPos );
        }

        public void End()
        {
            
            if (curMapLst != null)
                curMapLst.Clear();
            mainGo.SetActive(false);
            if(dataCtrl!=null)
            {
                dataCtrl.Unload();
                dataCtrl = null;
            }
        }


        #endregion

        

      

        private void FreshMap()
        {
            //get need
            HashSet<Vector3Int> need=new HashSet<Vector3Int>();
            HashSet<Vector3Int> now=new HashSet<Vector3Int>();
            var viewSize = dataCtrl.mainData.viewSize;
            for (int i = viewCenter.x - viewSize.x; i < viewCenter.x + viewSize.x; i++)
            {    for (int j = viewCenter.y - viewSize.y ; j <= viewCenter.y+ viewSize.y; j++)
                {
                    for (int k = viewCenter.z - viewSize.z; k < viewCenter.z + viewSize.z; k++)
                    {
                        var pos = new Vector3Int(i, j, k);
                        if (!mapUtilCtrl.InArea(pos))
                            continue;
                        
                        need.Add(pos);
                    }
                }
            }
            var newMapLst = new List<MapUnitForm.Data>();
            foreach(var map in curMapLst)
            {
                if(!need.Contains(map.mapPos))
                {
                    map.unit.Hide();
                }
                else
                {
                    now.Add(map.mapPos);

                    newMapLst.Add(map);
                }
            }
            //fill
            foreach (var pos in need)
            {
                if (!now.Contains(pos))
                {
                    var map = dataCtrl.maps[pos.x, pos.y, pos.z];
                    map.unit.Show();

                    newMapLst.Add(map);
                }
            }
            curMapLst = newMapLst;

        }
        private void UpdateMapInfo()
        {
            //update
            foreach (var map in curMapLst)
            {
                map.unit.UpdateInfo();
            }
            
        }
        private void UpdateVision()
        {
            //Manage vison
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
        

        public Vector3 GetNavDir(Vector3 cur,Vector3 tar, int maxStep=99999)
        {
            return navigationCtrl.GetNextDir(cur,tar, maxStep);
        }

        public void UpdateInfo()
        {
            UpdateMapInfo();
            if((curCenterPos - viewCenter).sqrMagnitude>0.2f)
            {
                
                viewCenter = mapUtilCtrl.GetClosestInArea(curCenterPos);
                if (GlobalSettings.MAP_SHOW_DEBUG)
                {
                    Z_Log.Log("pos:" + curCenterPos + " to now cam Pos:"+viewCenter);
                }
                FreshMap();
            }
            UpdateVision();
        }
    }
}

