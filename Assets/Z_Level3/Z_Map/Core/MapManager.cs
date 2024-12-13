using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;
using Z_Map.Analysis;
using Z_UnitSystem;

namespace Z_Map
{
   

    public class MapManager : Z_MonoManager<MapManager>
    {
        public MapData data;
        public GameObject mainGo;

        public Dictionary<int, Unit> unitDic=new Dictionary<int, Unit>();
        

        public NavigationController navigationController;
        public MapUtilController mapUtilController;

        private Vector3Int curCenterPos;
        private Vector3Int viewCenter;

        private List<MapUnit> curMapLst = new List<MapUnit>();

        public override void Init()
        {
            base.Init();

            navigationController = new NavigationController();
            navigationController.Init(this);

            mapUtilController = new MapUtilController();
            mapUtilController.Init(this);
            
        }

        #region external

        public void Begin(MapData data)
        {
            End();
            Init();
            mainGo.SetActive(true);
            this.data = data;
            

            viewCenter = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);

            for (int i = 0; i < data.items.Count; i++)
            {
                CheckAndLoad(data.items[i]);
            }
            for (int i = 0; i < data.characters.Count; i++)
            {
                CheckAndLoad(data.characters[i]);
            }
            navigationController.Build();
            mainGo.SetActive(true);
        }
        public void AddUnit(Unit unit)
        {
            if(unit is ItemUnit item)
            {
                data.items.Add(item);
            }else if(unit is CharacterUnit character)
            {
                data.characters.Add(character);
            }

            CheckAndLoad(unit);
            unit.UpdateActive();
        }
        public void RemoveUnit(int uid)
        {
            var unit = unitDic[uid];
            if (unit is ItemUnit item)
            {
                data.items.Remove(item);
            }
            else if (unit is CharacterUnit character)
            {
                data.characters.Remove(character);
            }
            unit.superUnit.Unbind(unit);
            unit.VisOff();
            unit.Hide();
            unitDic.Remove(uid);
        }
        private bool CheckAndLoad(Unit unit)
        {
            var mapPos = mapUtilController.RealPos2MapPos(unit.pos);
            if (mapUtilController.InArea(mapPos))
            {
                var map = data.maps[mapPos.x, mapPos.y, mapPos.z];
                map.Bind(unit);
                Register(unit);
                return true;
            }
            return false;
        }
        public void SetPos(Vector3 curCenterPos)
        {
            this.curCenterPos = mapUtilController.RealPos2MapPos(curCenterPos );
        }

        public void End()
        {
            
            if (curMapLst != null)
                curMapLst.Clear();
            mainGo.SetActive(false);
            if(data!=null)
            {
                data.Unload();
                data = null;
            }
        }


        #endregion

        private void Register(Unit tar)
        {
            unitDic[tar.uid]= tar;
        }
        private void Unregister(Unit tar)
        {
            
            if (unitDic.ContainsKey(tar.uid))
            {
                unitDic.Remove(tar.uid);
            }
           
        }

      

        private void FreshMap()
        {
            //get need
            HashSet<Vector3Int> need=new HashSet<Vector3Int>();
            HashSet<Vector3Int> now=new HashSet<Vector3Int>();
            for (int i = viewCenter.x - data.viewSize.x; i < viewCenter.x + data.viewSize.x; i++)
            {    for (int j = viewCenter.y - data.viewSize.y ; j <= viewCenter.y+ data.viewSize.y; j++)
                {
                    for (int k = viewCenter.z - data.viewSize.z; k < viewCenter.z + data.viewSize.z; k++)
                    {
                        var pos = new Vector3Int(i, j, k);
                        if (!mapUtilController.InArea(pos))
                            continue;
                        
                        need.Add(pos);
                    }
                }
            }
            var newMapLst = new List<MapUnit>();
            foreach(var map in curMapLst)
            {
                if(!need.Contains(map.mapPos))
                {
                    HideMap(map);
                }else
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
                    var map = data.maps[pos.x, pos.y, pos.z];
                    ShowMap(map);
 
                    newMapLst.Add(map);
                }
            }
            curMapLst = newMapLst;

        }
        private void UpdateMap()
        {
            //update
            foreach (var map in curMapLst)
            {
                map.UpdateInfo();
            }
            //Manage vison
            foreach (var curMap in curMapLst)
            {
                if (curMap.mapPos.y <= viewCenter.y)
                {
                    curMap.VisOn();
                }
                else
                {
                    curMap.VisOff();
                }
            }
        }
        private void HideMap(MapUnit map)
        {
            map.Hide();
        }
        private void ShowMap(MapUnit map)
        {
            map.Show();
        }

        public Vector3 GetNavDir(Vector3 cur,Vector3 tar, int maxStep=99999)
        {
            return navigationController.GetNextDir(cur,tar, maxStep);
        }

        public void UpdateInfo()
        {
            UpdateMap();
            if((curCenterPos - viewCenter).sqrMagnitude>0.2f)
            {
                
                viewCenter = mapUtilController.GetClosestInArea(curCenterPos);

                FreshMap();
            }
        }
    }
}

