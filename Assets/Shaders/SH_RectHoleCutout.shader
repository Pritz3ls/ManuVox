Shader "UI/SH_StencilHoleRectMask"
{
    Properties
    {
        _OverlayColor("Overlay Color", Color) = (0, 0, 0, 0.8)
        _HoleRadius("Hole Radius (0-0.5)", Float) = 0.4
        _Smoothness("Edge Smoothness", Float) = 0.02
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            CGPROGRAM
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
                float4 vertex : SV_POSITION;
            };

            fixed4 _OverlayColor;
            float _HoleRadius;
            float _Smoothness;

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
                float2 center = float2(0.5, 0.5); // center of this UI element
                float dist = distance(uv, center);

                float alpha = smoothstep(_HoleRadius, _HoleRadius - _Smoothness, dist);

                return fixed4(_OverlayColor.rgb, _OverlayColor.a * alpha);
            }
            ENDCG
        }
    }
}
