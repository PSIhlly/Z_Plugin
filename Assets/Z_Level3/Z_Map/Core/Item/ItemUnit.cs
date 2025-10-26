using System;
using UnityEngine;
using Z_Map.Form;
using Z_Math;

namespace Z_Map
{
    public partial class ItemUnit : MapUnit
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
        public override void Show()
        {
            base.Show();

            Z_EventHelper.Invoke(new ItemEvent()
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

            Z_EventHelper.Invoke(new ItemEvent()
            {
                type = MapEventType.AfterUpdate,
                unit = this
            });
        }
        public override void Remove()
        {
            ItemUnitForm.RemoveData(data.uid);
            base.Remove();
        }

    }
}
