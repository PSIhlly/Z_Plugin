using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
using Z_Map.Form;
using Z_UnitSystem;

namespace Z_Map
{
    public class ItemUnit:Unit
    {
        public ItemUnit(ItemUnitForm.Data data) : base(data)
        {
        }
        public ItemUnitForm.Data data => (ItemUnitForm.Data)_data;

        public ItemInstance ins
        {
            set { base.ins = value; }
            get { return (ItemInstance)base.ins; }
        }


        public override Type GetInsType()
        {
            return typeof(ItemInstance);
        }

        public override void UpdateInfo()
        {
            if (isShowing)
            {
                data.pos = ins.transform.position;
                data.euler = ins.transform.eulerAngles;

                var newMapPos = MapManager.instance.utilCtrl.RealPos2MapPos(data.pos);
                if (MapManager.instance.utilCtrl.InArea(newMapPos))
                {
                    var newMap = MapManager.instance.data.maps[(newMapPos.x, newMapPos.y, newMapPos.z)];
                    if (superUnit != newMap.unit)
                    {
                        superUnit.Unbind(this);
                        newMap.unit.Bind(this);
                        SubUpdateActive();
                    }
                }else
                {
                    data.pos = MapManager.instance.utilCtrl.GetClosestInArea(data.pos);
                    ins.transform.position = data.pos;
                }
            }
        }
        public override void Remove()
        {
            ItemUnitForm.RemoveData(data.uid);
            base.Remove();
        }

        }
}
