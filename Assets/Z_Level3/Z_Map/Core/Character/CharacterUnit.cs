using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
using Z_UnitSystem;

namespace Z_Map
{

    public class CharacterUnit : Unit
    {
        public CharacterUnit(JObject jo):base(jo)
        {
            LoadJsonData(jo);
            if (isMine) navEnabled = false;
        }
        public CharacterUnit(int uid, int prefabId_Data, Vector3 pos, Vector3 eular, Vector3 scale, bool isMine =false, int alertDis = 9999, int pathDis = 9999, UpdateType updateType = UpdateType.Always) : base(uid, prefabId_Data, pos, eular, scale, updateType)
        {
            this.isMine = isMine;
            if (isMine) navEnabled = false;
            this.alertDis = alertDis;
            this.pathDis = pathDis;
        }
        public CharacterInstance ins
        {
            set { base.ins = value; }
            get { return (CharacterInstance)base.ins; }
        }


        public bool navEnabled=true;

        public Vector3 destination;
        public float speed = 1;
        public int alertDis;
        public int pathDis;
        public bool isMine;

        public override Type GetInsType()
        {
            return typeof(CharacterInstance);
        }

        public override void UpdateInfo()
        {
            

            if (updateType== UpdateType.Always||isShowing)
            {
                //nav
                if (navEnabled)
                {
                    if((destination-pos).sqrMagnitude< alertDis* alertDis)
                    {
                        Vector3 dir = MapManager.instance.GetNavDir(pos, destination, pathDis);
                        ins.transform.position = ins.transform.position + dir * Time.deltaTime * speed;
                    }
                    
                }
                
                var newMapPos = MapManager.instance.mapUtilController.RealPos2MapPos(pos);
                if (MapManager.instance.mapUtilController.InArea(newMapPos))
                {
                    
                    var newMap = MapManager.instance.data.maps[newMapPos.x, newMapPos.y, newMapPos.z];
                    if(superUnit!=newMap)
                    {
                        superUnit.Unbind(this);
                        newMap.Bind(this);
                        UpdateActive();
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
                newPos = MapManager.instance.mapUtilController.GetClosestInArea(newPos);
                ins.transform.position = newPos;
                
                pos = ins.transform.position;
                euler = ins.transform.eulerAngles;

            }
        }
        private void LoadJsonData(JObject jo)
        {
            isMine= jo.Get<bool>("isMine");
            alertDis=jo.Get<int>("alertDis");
            pathDis=jo.Get<int>("pathDis");
        }
        public override JObject GetJsonData()
        {
            JObject jo = base.GetJsonData();
            jo.Set("isMine",isMine);
            jo.Set("alertDis",alertDis);
            jo.Set("pathDis", pathDis);
            return jo;
        }
    }
}


