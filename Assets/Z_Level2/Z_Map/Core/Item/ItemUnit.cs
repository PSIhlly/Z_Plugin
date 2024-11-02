using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_Map
{
    public class ItemUnit:Unit
    {
        public ItemUnit(int uid, GameObject prefab, Vector3 pos, Vector3 eular, bool isObstacle,UpdateType updateType = UpdateType.ShowOnly): base(uid,prefab, pos, eular, updateType)
        {
            this.isObstacle = isObstacle;
        }
        public ItemInstance ins
        {
            set { base.ins = value; }
            get { return (ItemInstance)base.ins; }
        }

        public MapUnit map;
        public bool isObstacle;

        public void Show()
        {
            if(ins==null||ins.gameObject==null)
            {
                var go= GameObject.Instantiate(prefab);
                go.transform.SetParent(MapManager.instance.mainGo.transform);
                ins = go.GetComponent<ItemInstance>();
            }
            ins.gameObject.SetActive(true);
            ins.transform.position=pos;
            ins.transform.eulerAngles = eular;
        }
        public void Hide()
        {
            if (ins == null || ins.gameObject == null)
                return;
            ins.gameObject.SetActive(false);
        }

        public void UpdateInfo()
        {
            if (ins != null && ins.gameObject.activeSelf)
            {
                pos = ins.transform.position;
                eular = ins.transform.eulerAngles;

                var newMapPos = MapManager.instance.mapUtilController.RealPos2MapPos(pos);
                if (MapManager.instance.mapUtilController.InArea(newMapPos))
                {
                    map.Unbind(this);
                    var newUnit = MapManager.instance.maps[newMapPos.x, newMapPos.y, newMapPos.z];
                    newUnit.Bind(this);
                    if (!newUnit.isShowing)
                    {
                        Hide();
                    }
                }
            }
        }

    }
}
