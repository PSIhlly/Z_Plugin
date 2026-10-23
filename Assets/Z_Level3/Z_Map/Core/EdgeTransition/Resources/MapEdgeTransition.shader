Shader "Z_Map/EdgeTransition"
{
    // Full-screen black overlay: samples a distance-field mask built from the map's
    // tile columns and darkens wherever the ground is close to a missing tile.
    // All parameters are written as globals by MapEdgeTransition; nothing lives in
    // the material or an MPB.
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+10"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "EdgeTransition"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MapEdgeMask);
            // Paired sampler: binds the texture's own filter/wrap (Bilinear + Clamp),
            // the same pattern URP uses for global camera textures.
            SAMPLER(sampler_MapEdgeMask);

            float2 _MapEdgeOrigin;    // Ground XZ at screen uv (0,0)
            float2 _MapEdgeRight;     // Ground XZ delta from uv.x 0 to 1
            float2 _MapEdgeUp;        // Ground XZ delta from uv.y 0 to 1
            float2 _MapEdgeCellSize;  // World size of one grid cell
            float2 _MapEdgeUvScale;   // 1 / mask size in cells
            float2 _MapEdgeUvOffset;  // (-maskOrigin + half cell) / size in cells
            float _MapEdgeStrength;   // Maximum opacity at the border
            float _MapEdgeMaskMin;    // texel / (fade + 2 texel)
            float _MapEdgeMaskScale;  // (fade + 2 texel) / fade
            float _MapEdgeBlackStart; // blackWidth / fadeWidth
            float _MapEdgeRampScale;  // 1 / (1 - blackStart)
            float _MapEdgeDebug;      // 1 = paint the no-tile area purple instead of the fade

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 ground = _MapEdgeOrigin + _MapEdgeRight * input.uv.x + _MapEdgeUp * input.uv.y;
                float2 maskUv = ground / _MapEdgeCellSize * _MapEdgeUvScale + _MapEdgeUvOffset;
                // The mask stores (distance + one texel) / (fade + 2 texel), i.e. a window
                // that is not clipped on either side of the border, so bilinear filtering
                // reproduces the real distance exactly. Undo it here: 0 at the border, 1
                // exactly fadeWidth cells in, clamped outside.
                float packed = SAMPLE_TEXTURE2D(_MapEdgeMask, sampler_MapEdgeMask, maskUv).a;
                float coverage = saturate((packed - _MapEdgeMaskMin) * _MapEdgeMaskScale);
                // Debug view: flat purple where the map has no Tile at all (coverage 0),
                // so its edge must sit exactly on the tile border.
                if (_MapEdgeDebug > 0.5)
                {
                    float empty = 1.0 - step(0.001, coverage);
                    return half4(0.55, 0.0, 0.8, empty * _MapEdgeStrength);
                }
                // Ramp shape fitted from the UI gradient com_3sld_bg_jianbian_01_U
                // (32x64, RGB white, alpha 0 -> 255 along +Y). Its alpha curve is
                // p(x) = x + x(1-x)(-0.107868 + 2.339329x - 1.058959x^2 - 0.315335x^3),
                // a quintic pinned to p(0)=0 / p(1)=1 with max error 0.78/255, monotonic.
                // t = 0 on the tile border, so the bright end of that gradient (p = 1,
                // i.e. fully black here) sits on the border and the faded end runs inward.
                float t = saturate((coverage - _MapEdgeBlackStart) * _MapEdgeRampScale);
                float x = 1.0 - t;
                float profile = x + x * (1.0 - x) * (-0.107868 + x * (2.339329 + x * (-1.058959 + x * -0.315335)));
                float alpha = _MapEdgeStrength * saturate(profile);
                return half4(0.0, 0.0, 0.0, alpha);
            }
            ENDHLSL
        }
    }
}
