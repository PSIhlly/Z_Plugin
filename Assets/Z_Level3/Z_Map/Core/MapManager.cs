using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;
using Z_Map.Analysis;

namespace Z_Map
{
   

    public class MapManager : Z_MonoManager<MapManager>
    {
        public MapData data;
        public GameObject mainGo;

        public Dictionary<int, ItemUnit> itemDic=new Dictionary<int, ItemUnit>();
        public Dictionary<int,CharacterUnit> characterDic = new Dictionary<int, CharacterUnit>();

        public InstancePool[] pools;

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
            pools = new InstancePool[data.prefabs.Count];
            for (int i = 0; i < data.prefabs.Count; i++)
            {
                pools[i] = new InstancePool(data.prefabs[i], mainGo.transform);
            }

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
        public void AddItemUnit(ItemUnit item)
        {
            data.items.Add(item);
            CheckAndLoad(item);
            item.UpdateActive();
        }
        public void AddCharacterUnit(CharacterUnit character)
        {
            data.characters.Add(character);
            CheckAndLoad(character);
            character.UpdateActive();
        }
        public void RemoveItemUnit(int uid)
        {
            itemDic[uid].map.Unbind(itemDic[uid]);
            itemDic[uid].VisOff();
            itemDic[uid].Hide();
            data.items.Remove(itemDic[uid]);
            itemDic.Remove(uid);
        }
        public void RemoveCharacterUnit(int uid)
        {
            characterDic[uid].map.Unbind(characterDic[uid]);
            characterDic[uid].VisOff();
            characterDic[uid].Hide();
            data.characters.Remove(characterDic[uid]);
            characterDic.Remove(uid);
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
            
            if (pools!=null)
            foreach(var pool in pools)
                pool.Destroy();
            if (curMapLst != null)
                curMapLst.Clear();
            mainGo.SetActive(false);

            foreach (Transform child in mainGo.transform)
            {
                Destroy(child.gameObject);
            }
        }


        #endregion

        private void Register(Unit tar)
        {
            if (tar is ItemUnit item)
            {
                itemDic[item.uid]=item;
            }
            else if (tar is CharacterUnit character)
            {
                characterDic[character.uid]=character;
            }
            else
            {
                Debug.LogError("Register cant find " + tar.GetType());
            }
        }
        private void Unregister(Unit tar)
        {
            if (tar is ItemUnit item)
            {
                itemDic.Remove(item.uid);
            }
            else if (tar is CharacterUnit character)
            {
                characterDic.Remove(character.uid);
            }
            else
            {
                Debug.LogError("unregister cant find " + tar.GetType());
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
            map.Show<MapInstance>();
        }

        public Vector3 GetNavDir(Vector3 cur,Vector3 tar, int maxStep=99999)
        {
            return navigationController.GetNextDir(cur,tar, maxStep);
        }

        public void LateUpdate()
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

