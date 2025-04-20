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
    public partial class ObjectUnit: MapUnit
    {
        public ObjectUnit(ObjectUnitForm.Data data) : base(data)
        {
        }
        public ObjectUnitForm.Data data => (ObjectUnitForm.Data)_data;

        public ObjectInstance ins
        {
            set { base.ins = value; }
            get { return (ObjectInstance)base.ins; }
        }
        

        public override Type GetInsType()
        {
            return typeof(ObjectInstance);
        }
        public override void Show()
        {
            base.Show();
            Z_EventHelper.Invoke(new ObjectEvent()
            {
                type = MapEventType.Show,
                unit = this
            });
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

            Z_EventHelper.Invoke(new ObjectEvent()
            {
                type = MapEventType.AfterUpdate,
                unit = this
            });
        }
        public override void Remove()
        {
            ObjectUnitForm.RemoveData(data.uid);
            base.Remove();
        }

        }
}
