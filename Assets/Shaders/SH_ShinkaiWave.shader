Shader "UI/ShinkaiWavesShader"
{
    Properties
    {
        _TimeSpeed("Time Speed", Float) = 1.0
        _WaveDirection ("Wave Direction (X,Y)", Vector) = (1, 0, 0, 0)
        _SkyTop("Sky Top", Color) = (0.05, 0.2, 0.5, 1)
        _SkyMid("Sky Mid", Color) = (0.5, 0.6, 0.8, 1)
        _Horizon("Horizon", Color) = (1.0, 0.8, 0.6, 1)
        _Ground("Ground", Color) = (0.2, 0.15, 0.3, 1)
        _DustColor("Dust Color", Color) = (1.0, 0.9, 0.7, 1)
        _GrainStrength("Grain Strength", Float) = 0.15
        _DustStrength("Dust Strength", Float) = 0.1
        _WaveScale("Wave Scale", Float) = 1.5
        _WaveSpeed("Wave Speed", Float) = 0.2
        _MainTex ("Main Texture (not used)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _TimeSpeed;
            float4 _SkyTop, _SkyMid, _Horizon, _Ground, _DustColor;
            float _GrainStrength, _DustStrength, _WaveScale, _WaveSpeed;
            float4 _WaveDirection;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 3.0;

                for (int i = 0; i < 5; ++i)
                {
                    value += amplitude * noise(p * frequency);
                    amplitude *= 0.5;
                    frequency *= 2.0;
                }

                return value;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 resolution = _ScreenParams.xy;
                float aspect = resolution.x / resolution.y;
                uv.x *= aspect;

                float time = _Time.y * _TimeSpeed;

                // Gradient position and wave
                // float gradientPos = uv.y + 0.2 * sin(uv.x * _WaveScale + time * _WaveSpeed);

                float2 dir = normalize(_WaveDirection.xy);
                float waveCoord = dot(uv, dir); // Align wave along direction
                float gradientPos = uv.y + 0.2 * sin(waveCoord * _WaveScale + time * _WaveSpeed);

                float3 color;
                if (gradientPos > 0.7)
                    color = lerp(_SkyTop.rgb, _SkyMid.rgb, (gradientPos - 0.7) / 0.3);
                else if (gradientPos > 0.4)
                    color = lerp(_SkyMid.rgb, _Horizon.rgb, (gradientPos - 0.4) / 0.3);
                else
                    color = lerp(_Ground.rgb, _Horizon.rgb, gradientPos / 0.4);

                // Horizontal variation
                color += float3(0.05, 0.02, 0.0) * sin(uv.x * 10.0 + time);

                // Grain
                float grain = noise(uv * 500.0 + time * 10.0) * _GrainStrength;

                // Dust
                float dust = fbm(uv * 3.0 + time * 0.1) * _DustStrength;
                color += dust * _DustColor.rgb;

                // Apply grain
                color += grain;

                // Vignette
                float vignette = 1.0 - smoothstep(0.5, 1.5, length(uv - 0.5) * 1.5);
                color *= lerp(0.8, 1.0, vignette);

                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}
