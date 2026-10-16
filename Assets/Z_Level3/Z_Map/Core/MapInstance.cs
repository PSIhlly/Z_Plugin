using System.Collections.Generic;
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
        private DisFadeRenderRouting.Route[] rendererRoutes;

        /// <summary>Reads the last visibility result without inspecting materials or property blocks.</summary>
        public DisFadeRenderRouting.Route GetRendererRoute(int rendererIndex)
        {
            return vising && rendererRoutes != null && (uint)rendererIndex < (uint)rendererRoutes.Length
                ? rendererRoutes[rendererIndex] : DisFadeRenderRouting.Route.Hidden;
        }

        private MaterialPropertyBlock visionBlock;
        private bool visionApplied;
        private float appliedSecondaryDegree;
        private int appliedDisplayLayer;
        private Unit visionOwner;
        private bool resetOnShow;
        private PerspectiveKeeper[] _viewRefreshKeepers;
        public virtual IReadOnlyList<PerspectiveKeeper> viewRefreshKeepers
        {
            get
            {
                if (_viewRefreshKeepers == null)
                {
                    _viewRefreshKeepers = GetComponentsInChildren<PerspectiveKeeper>(true);
                }
                return _viewRefreshKeepers;
            }
        }



        public override bool SupportsViewRefreshReuse
        {
            get
            {
                // Only the audited terrain implementation opts in. Subclasses,
                // Characters, decorations and arbitrary scripts retain the full
                // disable/enable lifecycle. Recheck the hierarchy so adding a
                // component after first use cannot silently bypass that policy.
                if (GetType() != typeof(TileInstance))
                    return false;
                return true;
            }
        }

        public override void PrepareForViewRefreshReuse()
        {
            PrepareForHide();
        }

        public override void PrepareForHide()
        {
            base.PrepareForHide();
            if (resetOnShow) return;
            resetOnShow = true;
            visionApplied = false;

        }

        public override void PrepareForShow()
        {
            if (!resetOnShow) return;
            resetOnShow = false;
            visionApplied = false;
            visionOwner = null;
            degree = 0f;
            appliedSecondaryDegree = 0f;
            vising = false;
            if (rendererRoutes != null)
                System.Array.Clear(rendererRoutes, 0, rendererRoutes.Length);
            for (int i = 0; i < viewRefreshKeepers.Count; i++)
                viewRefreshKeepers[i].PrepareForViewRefreshReuse();
        }



        protected virtual void OnEnable()
        {
            PrepareForShow();
            // Pool refresh restores the prefab's property blocks before reuse.
            visionApplied = false;
        }

        protected virtual void OnDisable()
        {
            PrepareForHide();
        }

        protected virtual void OnDestroy()
        {
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
            if (rendererRoutes == null || rendererRoutes.Length != renderers.Length)
            {
                rendererRoutes = new DisFadeRenderRouting.Route[renderers.Length];
            }
            for (int i = 0; i < renderers.Length; i++)
            {
                var render = renderers[i];
                if (renderers[i] == null)
                    continue;
                float rendererValue = GetRendererVisionDegree(i, value, secondaryValue);
                bool displayed = IsRendererVisibleInDisplayLayer(i);
                rendererRoutes[i] = !displayed || rendererValue <= 0f ? DisFadeRenderRouting.Route.Hidden
                    : rendererValue < 1f ? DisFadeRenderRouting.Route.Transparent : DisFadeRenderRouting.Route.Opaque;
                if (!displayed || rendererValue <= 0f)
                {
                    render.GetPropertyBlock(visionBlock);
                    visionBlock.SetFloat("_Show", 0f);
                    visionBlock.SetFloat("_FadeCenter", 0f);
                    DisFadeRenderRouting.Apply(render, visionBlock, rendererRoutes[i]);
                    continue;
                }
                // Texture/animation code shares this block. Read before modifying
                // visibility so a reused block never overwrites another renderer's values.
                render.GetPropertyBlock(visionBlock);
                // Occlusion uses uniform opacity, including independently visible Tile fronts.
                // Reset an earlier authored/pooled center fade when applying map visibility.
                visionBlock.SetFloat("_Show", rendererValue);
                visionBlock.SetFloat("_FadeCenter", 0f);
                ApplyRendererDisplayLayerProperties(render, i, visionBlock);
                DisFadeRenderRouting.Apply(render, visionBlock, rendererRoutes[i]);
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
