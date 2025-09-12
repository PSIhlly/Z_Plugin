using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
using Z_Map.Analysis;
using Z_Map.Form;
using Z_UnitSystem;
using Z_UnitSystem.Form;

namespace Z_Map
{

    public partial class CharacterUnit : MapUnit
    {
        public CharacterUnit(CharacterUnitForm.Data data) : base(data)
        {
        }
        public CharacterUnitForm.Data data => (CharacterUnitForm.Data)_data;

        public CharacterInstance ins
        {
            set { base.ins = value; }
            get { return (CharacterInstance)base.ins; }
        }

        public float pathDis;

        public override Type GetInsType()
        {
            return typeof(CharacterInstance);
        }
        public override void Show()
        {
            base.Show();
            Z_EventHelper.Invoke(new CharacterEvent()
            {
                type = MapEventType.Show,
                unit = this
            });
        }
        public override void UpdateInfo()
        {
            if (lastUpdateFrame == Time.frameCount)
                return;
            lastUpdateFrame = Time.frameCount;

            if (data.updateType == UpdateType.Always || isShowing)
            {
                //nav
                if (data.navEnabled)
                {
                    if ((data.destination - data.pos).sqrMagnitude < data.alertDis * data.alertDis)
                    {
                        Vector3 dir = MapManager.instance.GetNavDir(data.pos, data.destination, (int)data.pathDis);

                        ins.transform.position = ins.transform.position + dir * Time.deltaTime * data.speed;
                    }

                }

                var newPos = ins.transform.position;
                //gravity
                if (!Physics.Raycast(ins.transform.position + Vector3.up * 0.5f, Vector3.down, out var res, 0.6f, 1, QueryTriggerInteraction.Ignore))
                    newPos.y -= Time.deltaTime*2f;



                //collide
                MapManager.instance.utilCtrl.GetClosestInArea(newPos);

                //fix
                newPos = MapManager.instance.utilCtrl.GetClosestInArea(newPos);

                ins.transform.position = newPos;
                ins.step =   newPos - data.pos;
                
                data.pos = ins.transform.position;
                data.euler = ins.transform.eulerAngles;



                var newMapPos = MapManager.instance.utilCtrl.RealPos2MapPos(data.pos);
                if (MapManager.instance.utilCtrl.InArea(newMapPos))
                {
                    var newMap = MapManager.instance.data.maps[(newMapPos.x, newMapPos.y, newMapPos.z)].unit;
                    MapManager.instance.characterTileDic.Del(this);
                    MapManager.instance.characterTileDic.Add(this, newMap);
                }

            }

            Z_EventHelper.Invoke(new CharacterEvent()
            {
                type = MapEventType.AfterUpdate,
                unit = this
            });
        }
        public void Move(Vector3 dir)
        {
            if (ins != null)
            {
                var selfLength = ins.capsuleColliders[0].radius * ins.capsuleColliders[0].transform.localScale.x;
                if (Physics.Raycast(ins.transform.position + Vector3.up * 0.5f, dir, out var res, selfLength+dir.magnitude+0.01f,1, QueryTriggerInteraction.Ignore))
                {
                    
                    var dis = res.distance - selfLength;
                    if (dis < 0)
                        return;
                    dir *= dis / dir.magnitude;
                }
                ins.transform.position += dir;
            }
        }
        public override void Remove()
        {
            CharacterUnitForm.RemoveData(data.uid);
            base.Remove();
        }

    }
}


