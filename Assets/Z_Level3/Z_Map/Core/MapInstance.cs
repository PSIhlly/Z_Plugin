using UnityEngine;
using Z_Time;
using Z_UnitSystem;
using Z_UnitSystem.Form;

namespace Z_Map
{
    public partial class MapInstance : Instance
    {
        public MapUnit unit
        {
            set { base.unit = value; }
            get { return (MapUnit)base.unit; }
        }
        private float degree = 0;

        public Timer[] animTimer = new Timer[6];
        private PerspectiveKeeper[] _keepers;
        public PerspectiveKeeper[] keepers
        {
            get
            {
                if (_keepers == null)
                {
                    // Pooled/custom prefabs can keep the image hierarchy
                    // inactive until its material is assigned. Keep the
                    // perspective helpers discoverable so a same-frame object
                    // rotation can still be applied.
                    _keepers = GetComponentsInChildren<PerspectiveKeeper>(true);
                }
                return _keepers;
            }
        }
        protected virtual bool IsRendererVisibleInDisplayLayer(int rendererIndex)
        {
            return rendererIndex <= displayLayer;
        }

        public override void VisOn()
        {

            if (vising)
                return;
            vising = true;
            for (int i = 0; i < renderers.Length; i++)
            {
                var render = renderers[i];
                var isVisible = IsRendererVisibleInDisplayLayer(i);
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                render.GetPropertyBlock(propBlock);
                propBlock.SetFloat("_Show", isVisible ? 1 : 0);
                render.SetPropertyBlock(propBlock);
            }
            degree = 1;
        }
        public override void VisDegree(float degree)
        {

            if (degree == this.degree)
                return;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (!IsRendererVisibleInDisplayLayer(i))
                    continue;
                var render = renderers[i];
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                render.GetPropertyBlock(propBlock);
                propBlock.SetFloat("_Show", degree);
                render.SetPropertyBlock(propBlock);
            }
            this.degree = degree;

        }
        public override void VisOff()
        {
            if (!vising)
                return;
            vising = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                var render = renderers[i];
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                render.GetPropertyBlock(propBlock);
                propBlock.SetFloat("_Show", 0);
                degree = 0;
                render.SetPropertyBlock(propBlock);
            }
        }
    }
}
