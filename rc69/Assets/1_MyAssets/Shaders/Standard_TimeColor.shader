Shader "Custom/Standard_TimeColor_Overlay"
{
    Properties
    {
        _HueSpeed ("Hue Speed", Float) = 0.5
        _Saturation ("Saturation", Range(0,2)) = 1
        _Value ("Brightness", Range(0,2)) = 1
        _Intensity ("Overlay Alpha", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        CGPROGRAM
        #pragma surface surf Standard alpha
        #pragma target 3.0

        float _HueSpeed;
        float _Saturation;
        float _Value;
        float _Intensity;

        struct Input
        {
            // No UVs needed for flat color overlay
            float2 uv_dummy;
        };

        float3 HSVtoRGB(float3 hsv)
        {
            float4 K = float4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
            float3 p = abs(frac(hsv.xxx + K.xyz) * 6.0 - K.www);
            return hsv.z * lerp(K.xxx, saturate(p - K.xxx), hsv.y);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float hue = frac(_Time.y * _HueSpeed);
            float3 rgb = HSVtoRGB(float3(hue, _Saturation, _Value));

            o.Albedo = rgb;
            o.Alpha  = _Intensity;   // preserves base material details
        }
        ENDCG
    }

    FallBack Off
}
