Shader "TestShadrCode"
{
    Properties
    {
        // 基础纹理与颜色
        _MainTex ("Main Texture (RGBA)", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,0.5) // 默认半透（Alpha=0.5）
        
        // 半透明参数
        _AlphaScale ("Alpha Scale", Range(0,1)) = 1.0 // 整体透明度缩放
        
        // 光照参数
        _SpecularPower ("Specular Power", Range(1, 200)) = 50 // 镜面反射强度
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"       // 半透明专属RenderType
            "RenderPipeline"="UniversalPipeline" // URP标识
            "Queue"="Transparent"            // 半透明队列（在不透明物体后渲染）
            "IgnoreProjector"="True"         // 忽略投影器（半透明物体通常不接收投影）
            "SRPBatcherCompatible"="True"    // 兼容SRP Batcher
        }

        // 关闭深度写入（半透明核心：避免遮挡后续半透明物体）
        ZWrite On
        // 开启深度测试（避免半透明物体穿透不透明物体）
        ZTest LEqual
        // 标准Alpha混合公式：源Alpha * 源颜色 + (1-源Alpha) * 目标颜色
        Blend SrcAlpha OneMinusSrcAlpha
        // 可选：双面渲染（如玻璃/水，注释掉则只渲染正面）
        //Cull Off

        Pass
        {
            Name "Universal Forward"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // 光照与阴影变体（必须加，否则阴影/光照失效）
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #define _ALPHATEST_ON 1
            // URP核心头文件
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            // 材质属性（打包到CBUFFER，兼容SRP Batcher）
            CBUFFER_START(UnityPerMaterial)
                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _Color;
                float _AlphaScale;
                float _SpecularPower;
            CBUFFER_END

            // 顶点输入结构体
            struct Attributes
            {
                float4 positionOS : POSITION; // 模型空间位置
                float2 uv : TEXCOORD0;        // UV坐标
                float3 normalOS : NORMAL;     // 模型空间法线
            };

            // 顶点输出/片元输入结构体
            struct Varyings
            {
                float2 uv : TEXCOORD0;                // UV坐标
                float4 positionHCS : SV_POSITION;     // 裁剪空间位置
                float3 positionWS : TEXCOORD1;        // 世界空间位置
                float3 normalWS : TEXCOORD2;          // 世界空间法线
                float4 shadowCoord : TEXCOORD3;       // 阴影采样坐标
            };

            // 顶点着色器
            Varyings vert(Attributes input)
            {
                Varyings output;

                // 模型空间 → 世界空间 → 裁剪空间（URP标准转换）
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformWorldToHClip(output.positionWS);

                // UV坐标（支持缩放/偏移）
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                // 模型空间法线 → 世界空间法线
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);

                // 计算阴影采样坐标（用于接收阴影）
                output.shadowCoord = TransformWorldToShadowCoord(output.positionWS);

                return output;
            }

            // 片元着色器（核心：半透明计算）
            half4 frag(Varyings input) : SV_Target
            {
                // 1. 采样主纹理 + 叠加色调
                half4 texColor = tex2D(_MainTex, input.uv);
                half4 baseColor = texColor * _Color;

                // 2. 计算最终Alpha值（纹理Alpha * 色调Alpha * 透明度缩放）
                half finalAlpha = baseColor.a * _AlphaScale;
                // 限制Alpha范围在0~1之间
                finalAlpha = saturate(finalAlpha);

                // 3. 获取主光源（包含阴影信息）
                Light mainLight = GetMainLight(input.shadowCoord);
                // 阴影强度（0=完全阴影，1=无阴影）
                half shadowStrength = mainLight.shadowAttenuation;

                // 4. 漫反射计算（兰伯特模型）
                half3 normalWS = normalize(input.normalWS);
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 diffuse = mainLight.color * NdotL * baseColor.rgb * shadowStrength;

                // 5. 镜面反射计算（Phong模型）
                half3 viewDirWS = normalize(_WorldSpaceCameraPos - input.positionWS);
                half3 reflectDirWS = reflect(-mainLight.direction, normalWS);
                half specular = pow(saturate(dot(reflectDirWS, viewDirWS)), _SpecularPower);
                half3 specularColor = mainLight.color * specular * 0.5; // 镜面强度缩放

                // 6. 最终颜色（漫反射 + 镜面反射）
                half3 finalColor = diffuse + specularColor;

                // 7. 输出最终颜色（RGB=光照颜色，A=最终Alpha）
                return half4(finalColor, _AlphaScale);
            }
            ENDHLSL
        }

        // 可选：阴影投射Pass（若需要半透明物体投射阴影，取消注释）
        /*
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _Color;
                float _AlphaScale;
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
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = tex2D(_MainTex, input.uv);
                half finalAlpha = texColor.a * _Color.a * _AlphaScale;
                // 半透明阴影：Alpha低于0.5则剔除（避免阴影太淡）
                clip(finalAlpha - 0.5);
                return 0;
            }
            ENDHLSL
        }
        */
    }

    // 回退到URP内置半透明Shader（避免异常）
    FallBack "Hidden/Universal Render Pipeline/Transparent"
}