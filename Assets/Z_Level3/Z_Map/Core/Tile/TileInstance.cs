using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Time;
using Z_UnitSystem;

namespace Z_Map
{
    public class TileInstance : MapInstance
    {
        private Renderer[] _layerRenderers;

        public override Renderer[] renderers
        {
            get
            {
                if (_layerRenderers == null)
                    _layerRenderers = GetComponentsInChildren<Renderer>(true);
                return _layerRenderers;
            }
        }

        public TileUnit unit
        {
            set { base.unit = value; }
            get { return (TileUnit)base.unit; }
        }


    }
}
