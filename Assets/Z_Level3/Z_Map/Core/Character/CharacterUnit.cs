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

    public class CharacterUnit : Unit
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

        public override void UpdateInfo()
        {
           

                if (data.updateType== (int)UpdateType.Always||isShowing)
            {
                //nav
                if (data.navEnabled)
                {
                    if((data.destination- data.pos).sqrMagnitude< data.alertDis * data.alertDis)
                    {
                        Vector3 dir = MapManager.instance.GetNavDir(data.pos, data.destination, (int)data.pathDis);

                        ins.transform.position = ins.transform.position + dir * Time.deltaTime * data.speed;
                    }
                    
                }
                
                var newMapPos = MapManager.instance.utilCtrl.RealPos2MapPos(data.pos);
                if (MapManager.instance.utilCtrl.InArea(newMapPos))
                {
                    
                    var newMap = MapManager.instance.data.maps[(newMapPos.x, newMapPos.y, newMapPos.z)];
                    if(superUnit!=newMap.unit)
                    {
                        superUnit.Unbind(this);
                        newMap.unit.Bind(this);
                        SubUpdateActive();
                    }
                    
                }
                var newPos = ins.transform.position;

                /*                //模拟重力
                                var curMap = map;
                                while (curMap.scale == Vector3.zero)
                                {
                                    var down = new Vector3Int(curMap.mapPos.x, curMap.mapPos.y - 1, curMap.mapPos.z);
                                    if (MapManager.instance.mapUtilController.InArea(down))
                                        curMap = MapManager.instance.maps[down.x, down.y, down.z];
                                    else
                                        break;
                                }
                                if (Mathf.Abs(newPos.y- curMap.GetYByPoint(new Vector2(newPos.x-curMap.pos.x,newPos.z-curMap.pos.z))) > 0.05f)
                                    newPos.y -= Time.deltaTime;*/

                //fix
                newPos = MapManager.instance.utilCtrl.GetClosestInArea(newPos);
                ins.transform.position = newPos;
                
                data.pos = ins.transform.position;
                data.euler = ins.transform.eulerAngles;

            }
        }
        public override void Remove()
        {
            CharacterUnitForm.RemoveData(data.uid);
            base.Remove();
        }

    }
}


