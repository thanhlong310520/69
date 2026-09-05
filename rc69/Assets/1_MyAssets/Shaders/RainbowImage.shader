Shader "Custom/RainbowImage"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _RainbowSpeed ("Rainbow Speed", Float) = 1.0
        _RainbowScale ("Rainbow Scale", Float) = 1.0
        _Alpha ("Global Alpha", Range(0,1)) = 1.0
        _Blend ("Blend Amount", Range(0,1)) = 1.0 // 0 = sprite color, 1 = rainbow
        _WhiteTolerance ("White Tolerance", Range(0,1)) = 0.05 // how close to white to replace
        // Masking
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        // ✅ UI Maskable support
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        ColorMask [_ColorMask]
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color    : COLOR;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float4 color    : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _RainbowSpeed;
            float _RainbowScale;
            float _Alpha;
            float _Blend;
            float _WhiteTolerance;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color;
                return o;
            }

            // HSV → RGB
            float3 HSVtoRGB(float3 hsv)
            {
                float4 K = float4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
                float3 p = abs(frac(hsv.xxx + K.xyz) * 6.0 - K.www);
                return hsv.z * lerp(K.xxx, saturate(p - K.xxx), hsv.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Base sprite
                fixed4 texCol = tex2D(_MainTex, i.uv) * i.color;

                // Rainbow animation
                float hue = frac(_Time.y * _RainbowSpeed + i.uv.x * _RainbowScale);
                float3 rainbow = HSVtoRGB(float3(hue, 1.0, 1.0));

                // Detect white or near-white pixel
                float isWhite = step(1.0 - _WhiteTolerance, texCol.r) *
                                step(1.0 - _WhiteTolerance, texCol.g) *
                                step(1.0 - _WhiteTolerance, texCol.b);

                // Blend only on white pixels
                float3 finalRGB = lerp(texCol.rgb, rainbow, isWhite * _Blend);

                return fixed4(finalRGB, texCol.a * _Alpha);
            }
            ENDCG
        }
    }
}
