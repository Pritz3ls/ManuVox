Shader "UI/GreenPulsingBlobs_CustomColor"
{
    Properties
    {
        _TimeSpeed("Time Speed", Float) = 1.0
        _BlobColor("Blob Color", Color) = (0.117, 0.564, 1.0, 1.0) // #1E90FF
        _GlowColor("Glow Color", Color) = (0.05, 0.4, 0.8, 1.0)    // Softer blue
        _MainTex("Main Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _TimeSpeed;
            float4 _BlobColor;
            float4 _GlowColor;

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

            float random(float2 st)
            {
                return frac(sin(dot(st, float2(12.9898, 78.233))) * 43758.5453123);
            }

            float noise(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);

                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(a, b, u.x) +
                       (c - a) * u.y * (1.0 - u.x) +
                       (d - b) * u.x * u.y;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                float t = _Time.y * _TimeSpeed;
                float timeSegment = floor(t * 0.1);

                float2 randomPos = float2(
                    random(float2(timeSegment, 0.0)),
                    random(float2(0.0, timeSegment))
                );

                float dist = distance(uv, randomPos);

                float pulse = sin(t * 3.0) * 0.5 + 0.5;
                float circle = smoothstep(0.3 + pulse * 0.2, 0.0, dist);

                float noiseVal = noise(uv * 5.0 + t * 0.5) * 0.1;
                circle += noiseVal;

                float3 baseColor = _BlobColor.rgb * circle;
                float glow = smoothstep(0.8, 0.0, dist) * 0.3 * (sin(t) * 0.5 + 0.5);
                baseColor += _GlowColor.rgb * glow;

                return float4(baseColor, 1.0);
            }
            ENDCG
        }
    }
}
