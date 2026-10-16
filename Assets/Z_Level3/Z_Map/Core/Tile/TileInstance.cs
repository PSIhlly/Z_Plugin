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
            return rendererIndex >= LayerRendererCount && rendererIndex < LayerRendererCount * 2
                ? secondaryValue
                : primaryValue;
        }

        protected override bool IsRendererVisibleInDisplayLayer(int rendererIndex)
        {
            if (displayLayer == int.MaxValue)
                return true;

            // Tile renderers 0/1/2 are the normal parts and 3/4/5 are their
            // matching front parts. ModSceneMain's three display states are
            // therefore logical layers 0+3, 1+4 and 2+5.
            return GetLogicalLayer(rendererIndex) <= displayLayer;
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
