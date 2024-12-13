using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
using Z_UnitSystem;

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

        public bool isObstacle;

        public override Type GetInsType()
        {
            return typeof(ItemInstance);
        }

        public override void UpdateInfo()
        {
            if (isShowing)
            {
                pos = ins.transform.position;
                euler = ins.transform.eulerAngles;

                var newMapPos = MapManager.instance.mapUtilController.RealPos2MapPos(pos);
                if (MapManager.instance.mapUtilController.InArea(newMapPos))
                {
                    var newMap = MapManager.instance.data.maps[newMapPos.x, newMapPos.y, newMapPos.z];
                    if (superUnit != newMap)
                    {
                        superUnit.Unbind(this);
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
