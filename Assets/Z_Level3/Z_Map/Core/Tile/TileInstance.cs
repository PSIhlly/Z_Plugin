using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Time;
using Z_UnitSystem;

namespace Z_Map
{
    public class TileInstance : MapInstance
    {
        private const int LayerRendererCount = 3;
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

        protected override bool IsRendererVisibleInDisplayLayer(int rendererIndex)
        {
            if (displayLayer == int.MaxValue)
                return true;

            // Tile renderers 0/1/2 are the normal parts and 3/4/5 are their
            // matching front parts. ModSceneMain's three display states are
            // therefore logical layers 0+3, 1+4 and 2+5.
            var logicalLayer = rendererIndex;
            if (rendererIndex >= LayerRendererCount && rendererIndex < LayerRendererCount * 2)
                logicalLayer -= LayerRendererCount;
            return logicalLayer <= displayLayer;
        }

    }
}
