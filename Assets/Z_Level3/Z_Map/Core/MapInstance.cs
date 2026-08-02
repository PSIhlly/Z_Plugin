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

        public Timer[] animTimer = new Timer[3];
        private PerspectiveKeeper[] _keepers;
        public PerspectiveKeeper[] keepers
        {
            get
            {
                if (_keepers == null)
                {
                    _keepers = GetComponentsInChildren<PerspectiveKeeper>();
                }
                return _keepers;
            }
        }
        public override void VisOn()
        {

            if (vising)
                return;
            vising = true;
            for (int i = 0; i < renderers.Length; i++)
            {
                var render = renderers[i];
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                render.GetPropertyBlock(propBlock);
                propBlock.SetFloat("_Show", i <= displayLayer ? 1 : 0);
                degree = i <= displayLayer ? 1 : 0;
                render.SetPropertyBlock(propBlock);
            }
        }
        public override void VisDegree(float degree)
        {

            if (degree == this.degree)
                return;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (i > displayLayer)
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