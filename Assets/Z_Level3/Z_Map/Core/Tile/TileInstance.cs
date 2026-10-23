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
        private const float LowerLayerLightSensitivityScale = 0.5f;
        private const float HigherLayerPreviewOpacity = 0.5f;
        private static readonly int LightSensitivityProperty = Shader.PropertyToID("_LightSensitivity");
        private float[] _initialLightSensitivities;
        private bool[] _lightSensitivityCaptured;
        private bool[] _hasLightSensitivity;

        public TileUnit unit
        {
            set { base.unit = value; }
            get { return (TileUnit)base.unit; }
        }

        /// <summary>
        /// Applies the Tile body and front-part visibility independently.
        /// Renderer slots 0..2 are normal parts; 3..5 are front parts.
        /// </summary>
        public void ApplyVision(float value, float frontValue)
        {
            ApplyVisionValues(value, frontValue);
        }

        protected override float GetRendererVisionDegree(
            int rendererIndex,
            float primaryValue,
            float secondaryValue)
        {
            float value = rendererIndex >= LayerRendererCount && rendererIndex < LayerRendererCount * 2
                ? secondaryValue
                : primaryValue;
            // Preview upper texture layers without overriding stronger occlusion
            // or multiplying an already-half-transparent Tile a second time.
            return GetLogicalLayer(rendererIndex) > displayLayer
                ? Mathf.Min(value, HigherLayerPreviewOpacity)
                : value;
        }

        protected override bool IsRendererVisibleInDisplayLayer(int rendererIndex)
        {
            // Unlike the generic Renderer ceiling, Mod's texture-layer selector
            // keeps all normal/front pairs visible; upper pairs use half opacity.
            return true;
        }

        protected override DisFadeRenderRouting.Route GetRendererMaterialRoute(
            Renderer renderer, DisFadeRenderRouting.Route visibilityRoute)
        {
            // Keep an empty/disabled slot on its shared Hidden material. The
            // cached visibility route stays available when a texture enables it.
            return renderer.enabled ? visibilityRoute : DisFadeRenderRouting.Route.Hidden;
        }

        protected override void ApplyRendererDisplayLayerProperties(
            Renderer renderer,
            int rendererIndex,
            MaterialPropertyBlock block)
        {
            if (rendererIndex < 0 || rendererIndex >= LayerRendererCount * 2
                || !TryGetInitialLightSensitivity(renderer, rendererIndex, block, out float initialValue))
                return;

            int logicalLayer = GetLogicalLayer(rendererIndex);
            bool isLowerLayer = displayLayer != int.MaxValue && logicalLayer < displayLayer;
            block.SetFloat(
                LightSensitivityProperty,
                isLowerLayer ? initialValue * LowerLayerLightSensitivityScale : initialValue);
        }

        private bool TryGetInitialLightSensitivity(
            Renderer renderer,
            int rendererIndex,
            MaterialPropertyBlock block,
            out float value)
        {
            EnsureLightSensitivityCache();
            if (!_lightSensitivityCaptured[rendererIndex])
            {
                _lightSensitivityCaptured[rendererIndex] = true;
                if (block.HasFloat(LightSensitivityProperty))
                {
                    _initialLightSensitivities[rendererIndex] = block.GetFloat(LightSensitivityProperty);
                    _hasLightSensitivity[rendererIndex] = true;
                }
                else
                {
                    Material material = renderer.sharedMaterial;
                    if (material != null && material.HasProperty(LightSensitivityProperty))
                    {
                        _initialLightSensitivities[rendererIndex] = material.GetFloat(LightSensitivityProperty);
                        _hasLightSensitivity[rendererIndex] = true;
                    }
                }
            }

            value = _initialLightSensitivities[rendererIndex];
            return _hasLightSensitivity[rendererIndex];
        }

        private void EnsureLightSensitivityCache()
        {
            int count = renderers.Length;
            if (_initialLightSensitivities != null && _initialLightSensitivities.Length == count)
                return;

            _initialLightSensitivities = new float[count];
            _lightSensitivityCaptured = new bool[count];
            _hasLightSensitivity = new bool[count];
        }

        private static int GetLogicalLayer(int rendererIndex)
        {
            if (rendererIndex >= LayerRendererCount && rendererIndex < LayerRendererCount * 2)
                return rendererIndex - LayerRendererCount;
            return rendererIndex;
        }

    }
}
