using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;

namespace Z_Map
{
    public class ItemUnit:Unit
    {
        public ItemUnit(JObject jo) : base(jo)
        {
            LoadJsonData(jo);
        }
        public ItemUnit(int uid, int prefabId_Data, Vector3 pos, Vector3 eular, Vector3 scale, bool isObstacle,UpdateType updateType = UpdateType.ShowOnly): base(uid, prefabId_Data, pos, eular, scale, updateType)
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

        public void UpdateActive()
        {
            if (map == null)
                return;
            if (!map.isShowing)
            {
                if (isShowing)
                    Hide();
            }
            else if (!isShowing)
            {
                Show<ItemInstance>();
            }
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
                    var newMap = MapManager.instance.data.maps[newMapPos.x, newMapPos.y, newMapPos.z];
                    if (map != newMap)
                    {
                        map.Unbind(this);
                        newMap.Bind(this);
                        UpdateActive();
                    }
                }
            }
            
            
    }
        private void LoadJsonData(JObject jo)
        {
            isObstacle = jo.Get<bool>("isObstacle");
        }
        public override JObject GetJsonData()
        {
            JObject jo = base.GetJsonData();
            jo.Set("isObstacle", isObstacle);
            return jo;
        }
    }
}
