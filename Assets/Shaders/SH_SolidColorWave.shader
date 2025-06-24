Shader "UI/SolidColorWaveBands"
{
    Properties
    {
        _Color1 ("Color 1", Color) = (1, 0.8, 0.3, 1)
        _Color2 ("Color 2", Color) = (0.9, 0.3, 0.1, 1)
        _Color3 ("Color 3", Color) = (0.7, 0.1, 0.3, 1)
        _Color4 ("Color 4", Color) = (0.4, 0.1, 0.3, 1)
        _Color5 ("Color 5", Color) = (0.1, 0.1, 0.15, 1)

        _WaveDirection ("Wave Direction (X,Y)", Vector) = (1, 0, 0, 0)
        _WaveAmplitude ("Wave Amplitude", Float) = 0.15
        _WaveFrequency ("Wave Frequency", Float) = 4.0
        _BandThickness ("Band Thickness", Float) = 0.25
        _WaveSpeed ("Wave Scroll Speed", Float) = 0.5
        _MainTex ("Main Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        Pass
        {
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _Color1;
            float4 _Color2;
            float4 _Color3;
            float4 _Color4;
            float4 _Color5;

            float4 _WaveDirection;
            float _WaveSpeed;
            float _WaveAmplitude;
            float _WaveFrequency;
            float _BandThickness;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Normalize direction and project UV along wave direction
                float2 dir = normalize(_WaveDirection.xy);
                float waveCoord = dot(uv, dir);

                // 🔥 Animate the wave
                waveCoord += _Time.y * _WaveSpeed;

                // Apply sine wave distortion
                float waveOffset = sin(waveCoord * _WaveFrequency) * _WaveAmplitude;

                // Move bands along perpendicular axis
                float2 perpendicular = float2(-dir.y, dir.x);
                float bandCoord = dot(uv + perpendicular * waveOffset, perpendicular);

                // Band logic
                float band = floor(bandCoord / _BandThickness);

                if (band < 1.0) return _Color1;
                else if (band < 2.0) return _Color2;
                else if (band < 3.0) return _Color3;
                else if (band < 4.0) return _Color4;
                else return _Color5;
            }

            ENDCG
        }
    }
}
