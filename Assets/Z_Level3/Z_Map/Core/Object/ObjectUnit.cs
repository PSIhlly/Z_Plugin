using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
using Z_Map.Form;
using Z_Math;
using Z_UnitSystem;

namespace Z_Map
{
    public partial class ObjectUnit : MapUnit
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
                if (_data.pos != ins.transform.position || _data.euler != ins.transform.eulerAngles)
                    MapManager.instance.updateCtrl.ApplyMove(this, ins.transform.position, ins.transform.eulerAngles);

            }

            Z_EventHelper.Invoke(new ObjectEvent()
            {
                type = MapEventType.AfterUpdate,
                unit = this
            });
        }
        public void Move(Vector3 dir)
        {
            var mag = dir.magnitude;
            HashSet<int> exist = new HashSet<int>() { data.uid };
            float res = mag;
            Dictionary<CharacterUnit, Vector3> push=new Dictionary<CharacterUnit, Vector3>();
            if (dir != Vector3.zero)
            {
                foreach (var tile in manager.utilCtrl.GetOverlap(data))
                {
                    foreach (var ch in manager.updateCtrl.characterTileDic.Get(tile))
                    {
                        if (exist.Contains(ch.data.uid))
                            continue;
                        exist.Add(ch.data.uid);
                        var dis = manager.updateCtrl.CheckCollide(this, ch, dir, CollideType.CollideOnly);
                        if (dis < mag && dis>0 )
                        {
                            push[ch] = dir / mag * (dis - mag);
                        }
                    }
                }
            }
            manager.updateCtrl.ApplyMove(this, data.pos + dir, data.euler);
            foreach (var pair in push)
            {
                pair.Key.Move(pair.Value);
            }
        }
        public override void Remove()
        {
            ObjectUnitForm.RemoveData(data.uid);
            base.Remove();
        }

    }
}
