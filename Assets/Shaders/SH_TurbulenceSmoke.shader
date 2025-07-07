Shader "UI/ProceduralSmoke_NoMouse"
{
    Properties
    {
        _TimeSpeed("Time Speed", Float) = 1.0
        _SmokeColor("Smoke Color", Color) = (0.7, 0.7, 0.8, 1)
        _BackgroundColor("Background Color", Color) = (0.1, 0.1, 0.15, 1)
        _MainTex("Main Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
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

            float _TimeSpeed;
            float4 _SmokeColor;
            float4 _BackgroundColor;
            sampler2D _MainTex;
            float4 _MainTex_ST;

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

            float rand(float2 n)
            {
                return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = rand(i);
                float b = rand(i + float2(1.0, 0.0));
                float c = rand(i + float2(0.0, 1.0));
                float d = rand(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                for (int i = 0; i < 6; ++i)
                {
                    value += amplitude * noise(p);
                    p *= 2.0;
                    amplitude *= 0.5;
                }
                return value;
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

                float2 movement = float2(t * 0.1, t * 0.05);
                float turbulence = fbm(uv * 3.0 + movement);
                turbulence += fbm((uv + turbulence.xx) * 2.0 - movement);

                float smokeMask = fbm(uv * 1.5 + turbulence.xx + movement);
                smokeMask = smoothstep(0.2, 0.8, smokeMask);

                float3 finalColor = lerp(_BackgroundColor.rgb, _SmokeColor.rgb, smokeMask);

                return float4(finalColor, 1.0);
            }
            ENDCG
        }
    }
}
