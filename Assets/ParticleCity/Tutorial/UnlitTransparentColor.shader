Shader "Unlit/UnlitTransparentColor"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { 
            "RenderType" = "Transparent"
            "RenderQueue" = "Transparent" 
        }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
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

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color;
                col.rgb *= col.a;
                return col;
            }
            ENDCG
        }
    }
}
