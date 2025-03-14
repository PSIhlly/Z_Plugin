using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_UnitSystem;

namespace Z_Map
{
    public class MapInstance : Instance
    {
        public MapUnit unit
        {
            set { base.unit = value; }
            get { return (MapUnit)base.unit; }
        }
        public Dictionary<int,int> animCur=new Dictionary<int, int>();

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
                render.SetPropertyBlock(propBlock);
            }
        }
    }
}
