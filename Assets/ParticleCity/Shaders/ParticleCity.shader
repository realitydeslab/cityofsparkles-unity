Shader "Particle City/Particle City" 
{
    Properties 
    {
        _SpriteTex ("Base (RGB)", 2D) = "white" {}
        _SpriteColor("Sprite Color", Color) = (1, 1, 1, 1)
        _GlobalIntensity("Intencity", Range(0, 10)) = 1
        _Size ("Size", Range(0, 10)) = 0.5
        _PositionTex("Position Tex", 2D) = "white" {}
        _OffsetTex("Offset Tex", 2D) = "black" {}
        _NoiseTex("Noise Tex", 2D) = "white" {}
        _ColorPalleteTex("Color Pallete", 2D) = "white" {}
        _VolumeDeltaHeight("Volume Delta Height", Float) = 0
        _AlphaRandomWeight("Alpha Random Weight", Float) = 0.6
    }

    SubShader 
    {
        Pass
        {
            Tags 
            { 
                "RenderType" = "Transparent"
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
            }

            Cull Off
            Lighting Off
            ZWrite Off
            ZTest Off
            Blend One OneMinusSrcAlpha
        
            CGPROGRAM
                #pragma target 5.0
                #pragma vertex VS_Main
                #pragma fragment FS_Main
                #pragma geometry GS_Main
                #include "UnityCG.cginc" 

                // **************************************************************
                // Data structures                                                *
                // **************************************************************
                struct appdata {
                    float4 vertex   : POSITION;
                    float3 normal    : NORMAL;
                    float2 texcoord : TEXCOORD0;
                };

                struct GS_INPUT
                {
                    float4    pos        : POSITION;
                    float3    normal    : NORMAL;
                    float2  tex0    : TEXCOORD0;
                    float4  color   : COLOR0;
                };

                struct FS_INPUT
                {
                    float4  color   : COLOR0;
                    float4    pos        : POSITION;
                    float2  tex0    : TEXCOORD0;
                };


                // **************************************************************
                // Vars                                                            *
                // **************************************************************

                float _Size;
                float4x4 _VP;

                Texture2D _SpriteTex;
                float4 _SpriteColor;
                float _GlobalIntensity;
                float _VolumeDeltaHeight;
                float _AlphaRandomWeight;

                SamplerState sampler_SpriteTex;

                sampler2D _PositionTex;
                sampler2D _OffsetTex;
                sampler2D _NoiseTex;
                sampler2D _ColorPalleteTex;

                // **************************************************************
                // Shader Programs                                                *
                // **************************************************************

                // Vertex Shader ------------------------------------------------
                GS_INPUT VS_Main(appdata v)
                {
                    GS_INPUT output = (GS_INPUT)0;

                    float4 lodCoord = float4(v.texcoord, 0, 0);

                    float4 pos = tex2Dlod(_PositionTex, lodCoord);
                    float4 offset = tex2Dlod(_OffsetTex, lodCoord);
                    output.pos = float4(pos.xyz + offset.xyz, 1);

                    // Apply volume change

                    output.pos.y += _VolumeDeltaHeight * (max(0, pos.y - 80) / (250 - 80));

                    output.pos = mul(unity_ObjectToWorld, output.pos);

                    output.normal = v.normal;
                    output.tex0 = v.texcoord;

                    // Light effects

                    float4 lightCoord = lodCoord; // +float4(_Time.y / 512, 0, 0, 0);

                    float4 noise = tex2Dlod(_NoiseTex, lightCoord);
                    float lightNoise = noise.r;
                    float phase = noise.g;

                    float intense = clamp(sin(_Time.y + phase * 50) * 1.5 + 0.5, 0, 1);

                    // Color Pallete
                    float4 pallete = tex2Dlod(_ColorPalleteTex, noise.b);

                    // output.color.rgb = float3(1, 1, (1 - lightNoise * 0.5)) * _SpriteColor.xyz;
                    // output.color.a = ((1 - lightNoise) * 0.6 + lightNoise * intense) * _SpriteColor.a;
                    output.color = _SpriteColor * pallete * max(1, _GlobalIntensity);
                    output.color.a = ((1 - lightNoise) * _AlphaRandomWeight + lightNoise * intense) * _SpriteColor.a * min(1, _GlobalIntensity);

                    // Fog
                    // output.color.a *= 1 - 0.7 * saturate((distance(_WorldSpaceCameraPos, output.pos) - 100) / (1000 - 100));

                    return output;
                }

                // Geometry Shader -----------------------------------------------------
                [maxvertexcount(4)]
                void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
                {
                    float4 lodCoord = float4(p[0].tex0, 0, 0);
                    float4 noise = tex2Dlod(_NoiseTex, lodCoord);

                    float3 look = _WorldSpaceCameraPos - p[0].pos;
                    look = normalize(look);
                    float3 right = cross(look, float3(0, 1, 0));
                    right = normalize(right);
                    float3 up = cross(look, right);
                    up = normalize(up);

                    float halfS = 0.5f * _Size * noise.b * 1;
                            
                    float4 v[4];
                    v[0] = float4(p[0].pos + halfS * right - halfS * up, 1.0f);
                    v[1] = float4(p[0].pos + halfS * right + halfS * up, 1.0f);
                    v[2] = float4(p[0].pos - halfS * right - halfS * up, 1.0f);
                    v[3] = float4(p[0].pos - halfS * right + halfS * up, 1.0f);

#if UNITY_VERSION >= 560 
                    float4x4 vp = mul(UNITY_MATRIX_MVP, unity_WorldToObject);
#else 
#if UNITY_SHADER_NO_UPGRADE 
                    float4x4 vp = mul(UNITY_MATRIX_MVP, unity_WorldToObject);
#endif 
#endif

                    FS_INPUT pIn;
                    pIn.pos = mul(vp, v[0]);
                    pIn.tex0 = float2(1.0f, 0.0f);
                    pIn.color = p[0].color;
                    triStream.Append(pIn);

                    pIn.pos =  mul(vp, v[1]);
                    pIn.tex0 = float2(1.0f, 1.0f);
                    pIn.color = p[0].color;
                    triStream.Append(pIn);

                    pIn.pos =  mul(vp, v[2]);
                    pIn.tex0 = float2(0.0f, 0.0f);
                    pIn.color = p[0].color;
                    triStream.Append(pIn);

                    pIn.pos =  mul(vp, v[3]);
                    pIn.tex0 = float2(0.0f, 1.0f);
                    pIn.color = p[0].color;
                    triStream.Append(pIn);
                }



                // Fragment Shader -----------------------------------------------
                float4 FS_Main(FS_INPUT input) : COLOR
                {
                    float4 c = _SpriteTex.Sample(sampler_SpriteTex, input.tex0);
                    c *= input.color;

#if !defined(UNITY_COLORSPACE_GAMMA)
                    c.a = pow(c.a, 2.2);
#endif
                    c.rgb *= c.a;

                    return c;
                }

            ENDCG
        }
    } 
}
