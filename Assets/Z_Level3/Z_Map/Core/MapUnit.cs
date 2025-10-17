using UnityEngine;
using Z_UnitSystem;
using Z_UnitSystem.Form;

namespace Z_Map
{
    public partial class MapUnit : Unit
    {
        public MapManager manager => MapManager.instance;
        public MapUnit(UnitForm.Data data) : base(data)
        {
        }
        public TileUnit belongTile
        {
            get
            {
                if (this is ObjectUnit obj)
                {
                    return MapManager.instance.updateCtrl.objectTileDic.Get(obj)[0];
                }
                else if (this is ItemUnit item)
                {
                    return MapManager.instance.updateCtrl.itemTileDic.Get(item)[0];
                }
                else if (this is CharacterUnit character)
                {
                    return MapManager.instance.updateCtrl.characterTileDic.Get(character)[0];
                }
                return null;
            }

        }

        public MapInstance ins
        {
            set { base.ins = value; }
            get { return (MapInstance)base.ins; }
        }
        public override bool VisOn()
        {
            if (!base.VisOn())
                return false;
            switch (DynamicGlobalSettings.cameraMode)
            {
                case CameraMode.Top:
                    foreach (var render in ins.renderers)
                    {
                        render.transform.localPosition = Vector3.up * ins.transform.localScale.y / 2;
                        render.transform.localScale = Vector3.one;
                    }
                    break;

                case CameraMode.FortyFive:
                    foreach (var render in ins.renderers)
                    {
                        render.transform.localPosition = Vector3.up * ins.transform.localScale.y / 2;
                    }
                    break;
            }
            return true;
        }
    }
}