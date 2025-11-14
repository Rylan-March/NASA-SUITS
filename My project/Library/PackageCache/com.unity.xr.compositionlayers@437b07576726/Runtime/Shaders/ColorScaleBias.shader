Shader "Hidden/XRCompositionLayers/ColorScaleBias"
{
    Properties
    {
        _MainTex ("Screen Texture", 2D) = "white" {}
        _ColorScale ("Color Scale", Color) = (1,1,1,1)
        _ColorBias ("Color Bias", Color) = (0,0,0,0)
    }
    HLSLINCLUDE
        /// <summary>
        /// Color transformation function that applies scale and bias to a given color.
        /// </summary>
        inline float4 ApplyColorScaleBias(float4 color, float4 scale, float4 bias)
        {
            if (color.a > 0.0)
            {
                // Unmultiplies alpha for color correction
                color.rgb = color.rgb / color.a;
            }
            else
            {
                color.rgb = 0.0;
            }
            // Applies scale and bias
            color = color * scale + bias;
            // Re-multiplies alpha for proper blending
            color.rgb *= color.a;
            return color;
        }
    ENDHLSL

    // Universal Render Pipeline (URP) implementation
    SubShader
    {
        PackageRequirements
        {
            "com.unity.render-pipelines.core": "0.0"
            "com.unity.render-pipelines.universal" : "0.0"
        }
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "ForceNoShadowCasting" = "True"
            "IgnoreProjector" = "True"
        }
        LOD 100 // Level of Detail for shader
        Lighting Off
        Fog { Mode Off }
        ZWrite Off // Depth writing disabled for overlay effects
        Cull Off // Render both sides
        ZTest Always // Draw over existing content
        Blend SrcAlpha OneMinusSrcAlpha // Standard alpha blending
        Pass
        {
            Name "ColorScaleBiasEmulation_URP"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #define COMPOSITION_LAYERS_UNIVERSAL 1

    #if COMPOSITION_LAYERS_UNIVERSAL
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
    #endif

            float4 _ColorScale;
            float4 _ColorBias;
            /// <summary>
            /// Fragment shader for URP implementation
            /// Uses built-in blit texture sampling
            /// </summary>
            float4 Frag(Varyings input) : SV_Target
            {
                // Sample screen texture with proper filtering
                float4 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);
                return ApplyColorScaleBias(sceneColor, _ColorScale, _ColorBias);
            }
            ENDHLSL
        }
    }

    // Built-in Render Pipeline implementation
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Transparent" // Render after opaque objects
            "ForceNoShadowCasting" = "True"
            "IgnoreProjector" = "True"
        }
        LOD 100
        Lighting Off
        Fog { Mode Off }
        ZWrite Off
        Cull Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "ColorScaleBiasEmulation_BIRP"

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST; // Texture scale/offset
            float4 _ColorScale;
            float4 _ColorBias;

            /// <summary>
            /// Vertex shader for Built-in Render Pipeline
            /// Corrects Built-in Render Pipeline render texture vertical flip
            /// </summary>
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // Fixes inversion for Game View without flipping Scene View
                // Inverts verticaly when rendering to textures (_ProjectionParams.x > 0)
                // Maintain original UV for direct screen rendering
                o.uv.y = lerp(o.uv.y, 1 - o.uv.y, _ProjectionParams.x > 0);
                return o;
            }

            /// <summary>
            /// Fragment shader for Built-in Render Pipeline
            /// Uses standard texture sampling
            /// </summary>
            float4 frag (v2f i) : SV_Target
            {
                // Sample input texture
                float4 sceneColor = tex2D(_MainTex, i.uv);
                return ApplyColorScaleBias(sceneColor, _ColorScale, _ColorBias);
            }

            ENDHLSL
        }
    }
}
