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
            if(DynamicGlobalSettings.playing)
                data.unlock = true;

            Z_EventHelper.Invoke(new TileEvent()
            {
                type = MapEventType.Show,
                unit = this
            });
        }




        public float GetYByPoint(Vector2 selfPos)
        {
            if (data.scale == Vector3.zero)
                return -999;//直接坠落
            var collider=prefab.GetComponentInChildren<BoxCollider>();
            if(collider != null)
            {
                var euler = collider.transform.eulerAngles;
                euler.y += +data.euler.y;
                Vector3 normal = Quaternion.Euler(euler) * Vector3.back;
                if (Mathf.Abs(normal.y) < 1e-6f)
                    return data.pos.y;//平面接近竖直，y 不确定，退化返回 tile 中心高度

                //必经过点 (0, 1/tan(euler.x)/2, 0)
                //euler.x 接近 0 时 tan→0，1/tan 趋于无穷会产生 Infinity/NaN，需提前拦截
                float tan = Mathf.Tan(collider.transform.eulerAngles.x * Mathf.PI / 180f);
                if (Mathf.Abs(tan) < 1e-6f)
                    return data.pos.y;

                Vector3 planePoint = new Vector3(0, 1f / tan / 2f, 0);
                return data.pos.y + Z_Math.Graph.CalculateYAtPointInPlane(planePoint, normal, selfPos.x, selfPos.y);
            }
            return data.pos.y;
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
