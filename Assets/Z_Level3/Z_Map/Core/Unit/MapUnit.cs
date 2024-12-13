using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
using Z_UnitSystem;

namespace Z_Map
{

    public class MapUnit : Unit
    {
        public MapUnit(JObject jo) : base(jo)
        {
            LoadJsonData(jo);
        }
        public MapUnit(int uid, int prefabId_Data, Vector3 pos, Vector3 eular, Vector3 scale, int matId_Data, Vector3Int mapPos, UpdateType updateType = UpdateType.ShowOnly) : base(uid, prefabId_Data, pos, eular, scale, updateType)
        {
            this.matId_Data = matId_Data;
            this.mapPos = mapPos;
         }
        public MapInstance ins
        {
            set { base.ins = value; }
            get { return (MapInstance)base.ins; }
        }

        public string areaName;
        public int matId_Data;
        public Vector3Int mapPos;
        public Material mat=>MapManager.instance.data.materials[matId_Data];


        public override Type GetInsType()
        {
            return typeof(MapInstance);
        }

        public override void Show()
        {
            base.Show();

            if (ins.renderer)
                ins.renderer.material = mat;
            
        }
       
       
       
      
        public float GetYByPoint(Vector2 selfPos)
        {
            float length = 0;
            if (scale == Vector3.zero)
                return -999;//直接坠落
            if (euler.x != 0)
            {
                length = selfPos.y* (float)Math.Tan(-euler.x * 3.14f / 180);
            } else
            {
                length = selfPos.x * (float)Math.Tan(-euler.z * 3.14f / 180);
            }
            return pos.y + length;
        }

        private void LoadJsonData(JObject jo)
        {
            matId_Data = jo.Get<int>("matId_Data");
            mapPos = jo.Get<Vector3Int>("mapPos");
        }
        public override JObject GetJsonData()
        {
            JObject jo = base.GetJsonData();
            jo.Set("matId_Data", matId_Data);
            jo.Set("mapPos", mapPos);
            return jo;
        }

    }
}
