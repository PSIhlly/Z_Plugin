Shader "DisFadeCode"
{
    // ������壨��¶��Inspector��
    Properties
    {
        [NoScaleOffset]_Tex("_Tex", 2D) = "white" {}
        [NoScaleOffset]_AlphaTex("_AlphaTex", 2D) = "white" {}
        _UseCloseHide("_UseCloseHide", Float) = 0
        _Show("_Show", Float) = 0
        [Toggle] _FadeCenter("fadeCenter", Float) = 0
        _Alpha("_Alpha", Float) = 0
        _LightSensitivity("Light Sensitivity", Range(0.1, 10)) = 1
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Lit"
            "Queue"="Transparent"
            "ShaderGraphShader"="true"
        }

        Pass
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            // Continuous premultiplied transparency avoids opaque dither specks.
            // Transparent surfaces must not block later draws by writing depth.
            Blend One OneMinusSrcAlpha
            ZTest LEqual
            ZWrite Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            // ����������Ӱ��ص�multi_compile��������Ӱ����ʧЧ
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #define _ALPHATEST_ON 1

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                sampler2D _Tex;
                sampler2D _AlphaTex;
                float _Cutoff;
                float _Alpha;
                float _Show;
                float _FadeCenter;
                float _LightSensitivity;
            CBUFFER_END

            // Set by the active map. These are shared globals, not material overrides.
            float4 _MapFadeCenterPosition;

            half CameraCenterVisibility(float3 positionWS)
            {
                // Camera-local XY is centered on the actual view axis. Normalize
                // perspective positions at the map center's depth, when applicable.
                float3 positionVS = TransformWorldToView(positionWS);
                float2 viewOffset = positionVS.xy;
                if (unity_OrthoParams.w < 0.5)
                {
                    float centerDepth = abs(TransformWorldToView(_MapFadeCenterPosition.xyz).z);
                    viewOffset *= max(centerDepth, 0.0001) / max(abs(positionVS.z), 0.0001);
                }

                // Undo the camera's ground-plane projection so distances remain
                // world units regardless of camera tilt, zoom or map cell size.
                float2 rightXZ = UNITY_MATRIX_V[0].xz;
                float2 upXZ = UNITY_MATRIX_V[1].xz;
                float determinant = rightXZ.x * upXZ.y - rightXZ.y * upXZ.x;
                float2 groundOffset = viewOffset;
                if (abs(determinant) > 0.0001)
                    groundOffset = float2(
                        viewOffset.x * upXZ.y - viewOffset.y * rightXZ.y,
                        viewOffset.y * rightXZ.x - viewOffset.x * upXZ.x) / determinant;

                // Twenty-percent opacity through 1.5 world units, then a two-unit fade
                // ring reaching ordinary opacity at 3.5. Keep the quadratic curve.
                float visibility = saturate((length(groundOffset) - 1.5) / (3.5 - 1.5));
                return 0.2 + 0.8 * visibility * visibility;
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                half dark: TEXCOORD3;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformWorldToHClip(output.positionWS);
                output.uv = input.uv;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.dark =1;
                
                output.dark -= (_WorldSpaceCameraPos.y-output.positionWS.y-8)/10;
                

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 baseColor = tex2D(_Tex, input.uv);
                half4 alphaColor = tex2D(_AlphaTex, input.uv);

                #if _ALPHATEST_ON
                    clip(alphaColor.r- _Cutoff);
                    clip(baseColor.a - _Cutoff);
                #endif

                half centerVisibility = _FadeCenter > 0.5 ? CameraCenterVisibility(input.positionWS) : 1.0h;
                half finalAlpha = saturate(_Alpha * _Show * centerVisibility);
                // Discard only invisible fragments, never randomly retain opaque pixels.
                clip(finalAlpha - 0.0001h);

                // ���ģ�������Ӱ���꣨URP���ߺ�����
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);

                // 1. ��ȡ����Դ������������ɫ��
                Light mainLight = GetMainLight(shadowCoord); // ������Ӱ����
                // 2. ������Ӱǿ�ȣ�mainLight.shadowAttenuation������Ӱǿ�ȣ�0~1��
                // 0=��ȫ��Ӱ��1=����Ӱ
                half shadowStrength = mainLight.shadowAttenuation;

                half3 normalWS = normalize(input.normalWS);
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                // 光敏感性：_LightSensitivity越大，弱光下整体越亮
                half lightAmount = pow(NdotL, 1.0 / _LightSensitivity);
                // 3. 计算阴影强度衰减后的漫反射（降低模型被遮挡时的亮度）
                half3 diffuse = mainLight.color * lightAmount * baseColor.rgb * shadowStrength * input.dark;
                
                return half4(diffuse.rgb * finalAlpha, finalAlpha);
            }
            ENDHLSL
        }

        // ��ӰͶ��Pass�����䣬��֤Ͷ���ο���Ӱ��
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _ALPHATEST_ON
            
            #define _ALPHATEST_ON 1

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                sampler2D _Tex;
                sampler2D _AlphaTex;
                float _Cutoff;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 baseColor = tex2D(_Tex, input.uv);
                half4 alphaColor = tex2D(_AlphaTex, input.uv);
                #if _ALPHATEST_ON
                    // Visibility fading must not remove the caster. Keep only the
                    // authored texture/mask cutouts in the shadow silhouette.
                    clip(alphaColor.r - _Cutoff);
                    clip(baseColor.a - _Cutoff);
                #endif
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
