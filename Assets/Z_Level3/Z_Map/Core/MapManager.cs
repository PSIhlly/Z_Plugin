using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DataSystem.Form;
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
        public Vector3 sizeLimit=new Vector3(1000,1000,1000);
        public MapDataController dataCtrl;
        public GameObject mainGo;
        

        public NavigationController navigationCtrl;
        public MapUtilController utilCtrl;
        public MapUnitUtilController unitUtilCtrl;

        private Vector3Int curCenterPos;
        private Vector3Int viewCenter;

        private List<MapUnitForm.Data> curMapLst = new List<MapUnitForm.Data>();

        public override void Init()
        {
            base.Init();

            navigationCtrl = new NavigationController();
            navigationCtrl.Init(this);

            utilCtrl = new MapUtilController();
            utilCtrl.Init(this);

            unitUtilCtrl = new MapUnitUtilController();
            unitUtilCtrl.Init(this);
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
                var mapPos = utilCtrl.RealPos2MapPos(itemData.pos);
                if (utilCtrl.InArea(mapPos))
                {
                    MapUnitForm.DataByUid[dataCtrl.maps[(mapPos.x, mapPos.y, mapPos.z)].uid].unit.Bind(itemData.unit);
                }
            }
            foreach (var characterData in CharacterUnitForm.DataByUid.Values)
            {
                var mapPos = utilCtrl.RealPos2MapPos(characterData.pos);
                if (utilCtrl.InArea(mapPos))
                {
                    MapUnitForm.DataByUid[dataCtrl.maps[(mapPos.x, mapPos.y, mapPos.z)].uid].unit.Bind(characterData.unit);
                }
            }



            foreach(var alphaTexName in dataCtrl.mainData.alphaTexName)
            {
                var raws = new Texture2D[] {
                (Texture2D)TexAssetForm.DataByName["a$"+alphaTexName + "$0"]?.tex ,
                (Texture2D)TexAssetForm.DataByName["a$"+alphaTexName + "$1"]?.tex,
                (Texture2D)TexAssetForm.DataByName["a$"+alphaTexName + "$2"]?.tex,
                (Texture2D)TexAssetForm.DataByName["a$"+alphaTexName + "$3"]?.tex,
                (Texture2D)TexAssetForm.DataByName["a$"+alphaTexName + "$4"]?.tex,
                (Texture2D)TexAssetForm.DataByName["a$"+alphaTexName + "$5"]?.tex
                };
                unitUtilCtrl.CreateVariantsMatsByBasic5(alphaTexName, raws);
            }

            navigationCtrl.Build();
            mainGo.SetActive(true);
        }
        public MapUnitForm.Data AddMap(Vector3Int mapPos)
        {
            return dataCtrl.AddMap(mapPos);
        }
        public ItemUnitForm.Data AddItem(Vector3 realPos)
        {
            var mapPos = utilCtrl.RealPos2MapPos(realPos);
            if(!dataCtrl.maps.ContainsKey((mapPos.x, mapPos.y, mapPos.z)))
            {
                return null;
            }
            var data= dataCtrl.AddItem();
            data.pos = realPos;
            dataCtrl.maps[(mapPos.x, mapPos.y, mapPos.z)].unit.Bind(data.unit);
            return data;
        }

        public void SetPos(Vector3 curCenterPos)
        {
            this.curCenterPos = utilCtrl.RealPos2MapPos(curCenterPos );
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
                        if (!utilCtrl.InArea(pos))
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
                    var map = dataCtrl.maps[(pos.x, pos.y, pos.z)];
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
        public void ResetInfo(MapUnit unit=null)
        {
            foreach (var map in curMapLst)
            {
                map.unit.Hide();
            }
            foreach (var map in curMapLst)
            {
                map.unit.Show();
            }
            UpdateInfo(true);
        }
        public void UpdateInfo(bool forceFresh=false)
        {
            UpdateMapInfo();

            if (forceFresh||(curCenterPos - viewCenter).sqrMagnitude>0.2f)
            {
                
                viewCenter = curCenterPos;
                if (GlobalSettings.MAP_SHOW_DEBUG)
                {
                    Z_Log.Log("pos:" + curCenterPos + " to now cam Pos:"+viewCenter);
                }
                FreshMap();
            }
            UpdateVision();

            
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

