Shader "Unlit/XRCompositionLayers/Editor/Rects"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RectBounds ("Editor Rect Bounds", Vector) = (0,0,1,1)
        _SrcTexBounds ("Source Texture Bounds", Vector) = (0,0,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Fog { Mode Off }
            CGPROGRAM
            #pragma multi_compile COMPOSITION_SOURCE COMPOSITION_DEST
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 bound : TEXCOORD1;
                float2 size : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float4 _RectBounds;
            float4 _SrcTexBounds;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

    #if UNITY_UV_STARTS_AT_TOP
                o.uv = o.uv * float2(1.0, -1.0) + float2(0.0, 1.0);
    #endif

                o.bound.xy = _RectBounds.xy;
                o.bound.zw = o.bound.xy + _RectBounds.zw;
                o.size.xy = o.bound.zw;
                return o;
            }

            float inRect(float2 uv, float4 rect)
            {
                return (rect.x <= uv.x && uv.x <= rect.z) && (rect.y <= uv.y && uv.y <= rect.w);
            }

            float inDrag(float2 uv, float2 d, float2 texpt, float2 texres)
            {
                const float2 dragSize = 1 * texpt * (texres / 100);

                float2 td = uv - d;
                float dsqr = dot(td, td);

                return td.x < 0 && td.y < 0 && dsqr < dragSize;
            }

            float onRectBorder(float2 uv, float4 rect, float2 texpt, float2 texres)
            {
                const float2 borderSize = 1.5 * texpt * (texres / 100);

                float4 innerRect = float4(rect.xy + borderSize, rect.zw - borderSize);
                float4 outerRect = float4(rect.xy - borderSize, rect.zw + borderSize);

                if (inRect(uv, outerRect) && !inRect(uv, innerRect))
                {
                    float2 p1 = float2(rect.x, rect.y);
                    float2 p2 = float2(rect.z, rect.y);
                    float2 p3 = float2(rect.z, rect.w);
                    float2 p4 = float2(rect.x, rect.w);

                    float2 d1 = uv - p1;
                    float2 d2 = uv - p2;
                    float2 d3 = uv - p3;
                    float2 d4 = uv - p4;
                    d1.x *= d1.x;
                    d1.y *= d1.y;

                    d2.x *= d2.x;
                    d2.y *= d2.y;

                    d3.x *= d3.x;
                    d3.y *= d3.y;

                    d4.x *= d4.x;
                    d4.y *= d4.y;

                    float dd1 = d1.x + d1.y;
                    float dd2 = d2.x + d2.y;
                    float dd3 = d3.x + d3.y;
                    float dd4 = d4.x + d4.y;

                    float mind = min(dd1, min( dd2, min (dd3, dd4)));
                    return (mind <= borderSize);

                }

                return 0;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 srcStart = _SrcTexBounds.xy;
                float2 srcDim = _SrcTexBounds.zw;
                const float blend = 0.75;

#if COMPOSITION_SOURCE
                // sample the texture

                float2 sourceUv = srcStart + i.uv * srcDim;
#if UNITY_UV_STARTS_AT_TOP
                sourceUv = sourceUv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif

                fixed4 col = tex2D(_MainTex, sourceUv);
                col.rgb = LinearToGammaSpace(col.rgb);

                float2 texpt = _MainTex_TexelSize.xy;
                float2 texres = _MainTex_TexelSize.zw;
                float4 newCol = fixed4(1,1,1,.1);
                if (onRectBorder(i.uv, i.bound, texpt, texres))
                    newCol = float4(1,1,0,1);
                else if (inDrag(i.uv, i.size, texpt, texres))
                    newCol = float4(1,1,0,1);
                else if (inRect(i.uv, i.bound))
                    newCol = col;
                return (1 - blend) * col + blend * newCol;

#elif COMPOSITION_DEST
                // sample the texture

                float4 newCol = fixed4(.2,.2,.2,1);
                float4 col;

                if (inRect(i.uv, i.bound))
                {
                    float2 destWh = i.bound.zw - i.bound.xy;
                    float2 destUv = (i.uv - i.bound.xy)  / destWh;
                    float2 sourceUv = srcStart + destUv * srcDim;
#if UNITY_UV_STARTS_AT_TOP
                    sourceUv = sourceUv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
                    newCol = col = tex2D(_MainTex, sourceUv);
                    newCol.rgb = LinearToGammaSpace(col.rgb);
                }

                float2 texpt = _MainTex_TexelSize.xy;
                float2 texres = _MainTex_TexelSize.zw;
                col = newCol;
                if (inDrag(i.uv, i.size, texpt, texres))
                    newCol = float4(1,1,0,1);
                else if (onRectBorder(i.uv, i.bound, texpt, texres))
                    newCol = float4(1,1,0,1);
                return (1 - blend) * col + blend * newCol;
#endif //COMPOSITION
            }
            ENDCG
        }
    }
}
