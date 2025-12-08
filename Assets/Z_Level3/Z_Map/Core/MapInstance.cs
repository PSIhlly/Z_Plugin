using UnityEngine;
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
            foreach (var render in renderers)
            {
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                render.GetPropertyBlock(propBlock);
                propBlock.SetFloat("_Show", 1);
                degree = 1;
                render.SetPropertyBlock(propBlock);
            }
        }
        public override void VisDegree(float degree)
        {

            if (degree == this.degree)
                return;
            foreach (var render in renderers)
            {
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                render.GetPropertyBlock(propBlock);
                propBlock.SetFloat("_Show", degree);
                this.degree = degree;
                render.SetPropertyBlock(propBlock);
            }

        }
        public override void VisOff()
        {
            if (!vising)
                return;
            vising = false;
            foreach (var render in renderers)
            {
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                render.GetPropertyBlock(propBlock);
                propBlock.SetFloat("_Show", 0);
                degree = 0;
                render.SetPropertyBlock(propBlock);
            }
        }
    }
}