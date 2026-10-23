Shader "DisFadeCode"
{
    // ������壨��¶��Inspector��
    Properties
    {
        [NoScaleOffset]_Tex("_Tex", 2D) = "white" {}
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

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        // Textures belong to shared materials; only numeric per-renderer values
        // enter the MPB. The non-instanced variant retains UnityPerMaterial.
        sampler2D _Tex;
        UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
            UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
            UNITY_DEFINE_INSTANCED_PROP(float, _Alpha)
            UNITY_DEFINE_INSTANCED_PROP(float, _Show)
            UNITY_DEFINE_INSTANCED_PROP(float, _FadeCenter)
            UNITY_DEFINE_INSTANCED_PROP(float, _LightSensitivity)
        UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

        float4 _MapFadeCenterPosition;
        // Camera-scoped by DisFadeOpaqueFeature, never a material/MPB setting.
        float _DisFadeSplitEnabled;

        float CameraCenterVisibility(float3 positionWS)
        {
            float3 positionVS = TransformWorldToView(positionWS);
            float2 viewOffset = positionVS.xy;
            if (unity_OrthoParams.w < 0.5)
            {
                float centerDepth = abs(TransformWorldToView(_MapFadeCenterPosition.xyz).z);
                viewOffset *= max(centerDepth, 0.0001) / max(abs(positionVS.z), 0.0001);
            }

            float2 rightXZ = UNITY_MATRIX_V[0].xz;
            float2 upXZ = UNITY_MATRIX_V[1].xz;
            float determinant = rightXZ.x * upXZ.y - rightXZ.y * upXZ.x;
            float2 groundOffset = viewOffset;
            if (abs(determinant) > 0.0001)
                groundOffset = float2(
                    viewOffset.x * upXZ.y - viewOffset.y * rightXZ.y,
                    viewOffset.y * rightXZ.x - viewOffset.x * upXZ.x) / determinant;

            float visibility = saturate((length(groundOffset) - 1.5) / (3.5 - 1.5));
            return 0.2 + 0.8 * visibility * visibility;
        }

        float DisFadeOpacity(float3 positionWS)
        {
            float fadeCenter = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _FadeCenter);
            float alpha = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Alpha);
            float show = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Show);
            float visibility = fadeCenter > 0.5 ? CameraCenterVisibility(positionWS) : 1.0;
            return saturate(alpha * show * visibility);
        }
        ENDHLSL

        Pass
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            // Continuous premultiplied transparency avoids opaque dither specks.
            // Fully opaque pixels are drawn by DisFadeOpaque before this pass.
            // Only partially transparent surfaces reach this no-depth-write pass.
            Blend One OneMinusSrcAlpha
            ZTest LEqual
            ZWrite Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            // ����������Ӱ��ص�multi_compile��������Ӱ����ʧЧ
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #define _ALPHATEST_ON 1

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                half dark: TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
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
                UNITY_SETUP_INSTANCE_ID(input);
                half4 baseColor = tex2D(_Tex, input.uv);

                #if _ALPHATEST_ON
                    clip(baseColor.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
                #endif

                float finalAlpha = DisFadeOpacity(input.positionWS);
                // Discard only invisible fragments, never randomly retain opaque pixels.
                clip(finalAlpha - 0.0001h);
                if (_DisFadeSplitEnabled > 0.5 && finalAlpha >= 1.0)
                    discard;

                // ���ģ�������Ӱ���꣨URP���ߺ�����
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);

                // 1. ��ȡ����Դ������������ɫ��
                // URP uses distance fade to neutralize its empty shadow map.
                Light mainLight = GetMainLight(shadowCoord, input.positionWS, half4(1, 1, 1, 1));
                // 2. ������Ӱǿ�ȣ�mainLight.shadowAttenuation������Ӱǿ�ȣ�0~1��
                // 0=��ȫ��Ӱ��1=����Ӱ
                half shadowStrength = mainLight.shadowAttenuation;

                half3 normalWS = normalize(input.normalWS);
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                // 光敏感性：_LightSensitivity越大，弱光下整体越亮
                half lightAmount = pow(NdotL, 1.0 / UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _LightSensitivity));
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
            #pragma multi_compile_instancing
            #pragma multi_compile _ _ALPHATEST_ON
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            
            #define _ALPHATEST_ON 1

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            // URP supplies these with the current shadow-casting light.
            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                // Match URP's ShadowCasterPass: prevent the surface from
                // shadowing itself, including at large map coordinates.
                output.positionHCS = TransformWorldToHClip(
                    ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                #if UNITY_REVERSED_Z
                    output.positionHCS.z = min(output.positionHCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionHCS.z = max(output.positionHCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half4 baseColor = tex2D(_Tex, input.uv);
                #if _ALPHATEST_ON
                    // Visibility fading must not remove the caster. Keep only the
                    // authored texture cutouts in the shadow silhouette.
                    clip(baseColor.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
                #endif
                return 0;
            }
            ENDHLSL
        }

        // Executed by the active URP Render Objects feature after the skybox,
        // before any transparent color draws. Writing COLOR as well as depth
        // prevents a late opaque draw from overwriting translucent foreground.
        Pass
        {
            Name "DisFadeOpaque"
            Tags { "LightMode" = "DisFadeOpaque" }
            Blend Off
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                half dark : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformWorldToHClip(output.positionWS);
                output.uv = input.uv;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.dark = 1;
                output.dark -= (_WorldSpaceCameraPos.y - output.positionWS.y - 8) / 10;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half4 baseColor = tex2D(_Tex, input.uv);
                clip(baseColor.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
                // Exact split: even 0.9999 remains translucent and cannot write
                // depth. Fully hidden/fading and texture-cutout pixels skip it.
                clip(DisFadeOpacity(input.positionWS) - 1.0);

                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                // URP uses distance fade to neutralize its empty shadow map.
                Light mainLight = GetMainLight(shadowCoord, input.positionWS, half4(1, 1, 1, 1));
                half NdotL = saturate(dot(normalize(input.normalWS), mainLight.direction));
                half lightAmount = pow(NdotL, 1.0 / UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _LightSensitivity));
                half3 diffuse = mainLight.color * lightAmount * baseColor.rgb
                    * mainLight.shadowAttenuation * input.dark;
                return half4(diffuse, 1.0h);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
