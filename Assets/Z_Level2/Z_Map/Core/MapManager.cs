using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;
using Z_Map.Analysis;

namespace Z_Map
{
    public class MapManager : Z_MonoManager<MapManager>
    {
        public GameObject mainGo;
        public GameObject mapPrefab;

        public MapUnit[,,] maps
        {
            get; private set;
        }
        public Dictionary<int, ItemUnit> itemDic=new Dictionary<int, ItemUnit>();
        public Dictionary<int,CharacterUnit> characterDic = new Dictionary<int, CharacterUnit>();

        public MapInstancePool pool;

        public NavigationController navigationController;
        public MapUtilController mapUtilController;


        /// <summary>
        /// y means down
        /// </summary>
        private Vector3Int viewSize;


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

            pool = new MapInstancePool(mapPrefab, mainGo.transform);
        }

        #region external

        public void Begin(Vector3Int viewSize, MapUnit[,,] maps, ItemUnit[] items, CharacterUnit[] characters)
        {
            Init();
            this.viewSize = viewSize;
            viewCenter = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);

            this.maps = maps;

            for (int i = 0; i < items.Length; i++)
            {
                CheckAndLoad(items[i]);
            }
            for (int i = 0; i < characters.Length; i++)
            {
                CheckAndLoad(characters[i]);
            }

            navigationController.Build();
            mainGo.SetActive(true);
        }
       
        private bool CheckAndLoad(Unit unit)
        {
            var mapPos = mapUtilController.RealPos2MapPos(unit.pos);
            if (mapUtilController.InArea(mapPos))
            {
                var map = maps[mapPos.x, mapPos.y, mapPos.z];
                map.Bind(unit);
                Register(unit);
                return true;
            }
            return false;
        }
        public void SetPos(Vector3 curCenterPos)
        {
            this.curCenterPos = mapUtilController.RealPos2MapPos(curCenterPos);
        }

        public void End()
        {
            mainGo.SetActive(false);
            pool.Destroy();
            curMapLst.Clear();
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

      

        private void FreshMap(Vector3Int renderPos)
        {
            //get need
            HashSet<Vector3Int> need=new HashSet<Vector3Int>();
            HashSet<Vector3Int> now=new HashSet<Vector3Int>();
            for (int i = renderPos.x - viewSize.x; i < renderPos.x + viewSize.x; i++)
            {    for (int j = renderPos.y - viewSize.y + 1; j <= renderPos.y; j++)
                    //y means down
                {
                    for (int k = renderPos.z - viewSize.z; k < renderPos.z + viewSize.z; k++)
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
                if(!need.Contains(mapUtilController.RealPos2MapPos(map.pos)))
                {
                    HideMap(map);
                }else
                {
                    now.Add(mapUtilController.RealPos2MapPos(map.pos));

                    newMapLst.Add(map);
                }
            }
            //fill
            foreach (var pos in need)
            {
                if (!now.Contains(pos))
                {
                    var map = maps[pos.x, pos.y, pos.z];
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
        }
        private void HideMap(MapUnit map)
        {
            map.Hide();
        }
        private void ShowMap(MapUnit map)
        {
            map.Show();
        }

        public Vector3 GetNavDir(Vector3 cur,Vector3 tar)
        {
            return navigationController.GetNextDir(cur,tar);
        }

        public void LateUpdate()
        {
            UpdateMap();
            if((curCenterPos - viewCenter).sqrMagnitude>0.5f)
            {
                viewCenter = new Vector3Int((int)curCenterPos.x, (int)curCenterPos.y, (int)curCenterPos.z);
                FreshMap(viewCenter);
            }
        }
    }
}

