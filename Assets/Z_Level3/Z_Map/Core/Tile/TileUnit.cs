using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
using Z_DesignStyle;
using Z_Map.Form;
using Z_UnitSystem;

namespace Z_Map
{

    public partial class TileUnit : MapUnit
    {
        public TileUnit(TileUnitForm.Data data) : base(data)
        {
        }
        public TileUnitForm.Data data => (TileUnitForm.Data)_data;
        public TileInstance ins
        {
            set { base.ins = value; }
            get { return (TileInstance)base.ins; }
        }

        public override Type GetInsType()
        {
            return typeof(TileInstance);
        }

        public override void Show()
        {
            base.Show();


            Z_EventHelper.Invoke(new TileEvent()
            {
                type = MapEventType.Show,
                unit = this
            });
        }




        public float GetYByPoint(Vector2 selfPos)
        {
            float length = 0;
            if (data.scale == Vector3.zero)
                return -999;//直接坠落
            if (data.euler.x != 0)
            {
                length = selfPos.y * (float)Math.Tan(-data.euler.x * 3.14f / 180);
            }
            else
            {
                length = selfPos.x * (float)Math.Tan(-data.euler.z * 3.14f / 180);
            }
            return data.pos.y + length;
        }


        public override void Remove()
        {
            MapManager.instance.data.UnRegisterMap(data);
            TileUnitForm.RemoveData(data.uid);
            base.Remove();
        }

        public override void UpdateInfo()
        {
            base.UpdateInfo();

            Z_EventHelper.Invoke(new TileEvent()
            {
                type = MapEventType.AfterUpdate,
                unit = this
            });

        }


    }
}
