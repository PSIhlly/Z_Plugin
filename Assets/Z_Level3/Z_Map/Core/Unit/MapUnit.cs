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

    public class MapUnit : Unit
    {
        public MapUnit(MapUnitForm.Data data) : base(data)
        {
        }
        public MapUnitForm.Data data => (MapUnitForm.Data)_data;

        public string areaName;
        public int matId_Data;
        public Vector3Int mapPos;
        public Material mat=> InstancePoolManager.instance.GetMaterial(matId_Data);


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
            if (data.scale == Vector3.zero)
                return -999;//直接坠落
            if (data.euler.x != 0)
            {
                length = selfPos.y* (float)Math.Tan(-data.euler.x * 3.14f / 180);
            } else
            {
                length = selfPos.x * (float)Math.Tan(-data.euler.z * 3.14f / 180);
            }
            return data.pos.y + length;
        }
        
        
    }
}
