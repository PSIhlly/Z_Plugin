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
        private static readonly int ShowProperty = Shader.PropertyToID("_Show");
        private MaterialPropertyBlock visionBlock;
        private bool visionApplied;
        private float appliedSecondaryDegree;
        private int appliedDisplayLayer;
        private Unit visionOwner;

        protected virtual void OnEnable()
        {
            // Pool refresh restores the prefab's property blocks before reuse.
            visionApplied = false;
        }

        protected virtual void OnDisable()
        {
            visionApplied = false;
        }

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

        protected virtual void ApplyRendererDisplayLayerProperties(
            Renderer renderer,
            int rendererIndex,
            MaterialPropertyBlock block)
        {
        }

        public override void VisOn()
        {
            ApplyVision(vising && degree > 0f ? degree : 1f);
        }
        public override void VisDegree(float degree)
        {
            ApplyVision(degree);
        }
        public override void VisOff()
        {
            ApplyVision(0f);
        }

        /// <summary>Submit only a changed final visibility state, including display-layer/pool changes.</summary>
        public void ApplyVision(float value)
        {
            ApplyVisionValues(value, value);
        }

        /// <summary>
        /// Submit a primary and secondary visibility value. Derived instances decide
        /// which renderer slots, if any, use the secondary value.
        /// </summary>
        protected void ApplyVisionValues(float value, float secondaryValue)
        {
            value = Mathf.Clamp01(value);
            secondaryValue = Mathf.Clamp01(secondaryValue);
            bool visible = value > 0f || secondaryValue > 0f;
            if (visionApplied && degree == value && vising == visible
                && appliedSecondaryDegree == secondaryValue
                && appliedDisplayLayer == displayLayer && visionOwner == base.unit)
                return;

            if (visionBlock == null)
                visionBlock = new MaterialPropertyBlock();
            for (int i = 0; i < renderers.Length; i++)
            {
                var render = renderers[i];
                // Texture/animation code shares this block. Read before modifying
                // _Show so a reused block never overwrites another renderer's values.
                render.GetPropertyBlock(visionBlock);
                float rendererValue = GetRendererVisionDegree(i, value, secondaryValue);
                visionBlock.SetFloat(ShowProperty, IsRendererVisibleInDisplayLayer(i) ? rendererValue : 0f);
                ApplyRendererDisplayLayerProperties(render, i, visionBlock);
                render.SetPropertyBlock(visionBlock);
            }
            degree = value;
            vising = visible;
            appliedSecondaryDegree = secondaryValue;
            appliedDisplayLayer = displayLayer;
            visionOwner = base.unit;
            visionApplied = true;
        }

        protected virtual float GetRendererVisionDegree(
            int rendererIndex,
            float primaryValue,
            float secondaryValue)
        {
            return primaryValue;
        }
    }
}
