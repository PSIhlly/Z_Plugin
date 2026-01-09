Shader "DisFadeCode"
{
    // 属性面板（暴露给Inspector）
    Properties
    {
        [NoScaleOffset]_Tex("_Tex", 2D) = "white" {}
        [NoScaleOffset]_AlphaTex("_AlphaTex", 2D) = "white" {}
        _UseCloseHide("_UseCloseHide", Float) = 0
        _Show("_Show", Float) = 0
        _Alpha("_Alpha", Float) = 0
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

        Blend SrcAlpha OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            // 必须添加阴影相关的multi_compile，否则阴影函数失效
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
            CBUFFER_END

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
                    clip(_Show- _Cutoff);
                    clip(baseColor.a - _Cutoff);
                #endif

                // 核心：计算阴影坐标（URP工具函数）
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);

                // 1. 获取主光源（包含方向、颜色）
                Light mainLight = GetMainLight(shadowCoord); // 传入阴影坐标
                // 2. 采样阴影强度：mainLight.shadowAttenuation就是阴影强度（0~1）
                // 0=完全阴影，1=无阴影
                half shadowStrength = mainLight.shadowAttenuation;

                half3 normalWS = normalize(input.normalWS);
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                // 3. 用阴影强度衰减漫反射（核心：接收阴影的关键）
                half3 diffuse = mainLight.color * NdotL * baseColor.rgb * shadowStrength * input.dark;
                
                return half4(diffuse.rgb,saturate(_Alpha));
            }
            ENDHLSL
        }

        // 阴影投射Pass（不变，保证投射镂空阴影）
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
                float _Show;
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
                    clip(baseColor.a - _Cutoff);
                #endif
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}