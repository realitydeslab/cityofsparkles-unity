Shader "Particle City/Particle City" 
{
    Properties 
    {
        _SpriteTex ("Base (RGB)", 2D) = "white" {}
        _SpriteColor("Sprite Color", Color) = (1, 1, 1, 1)
        _GlobalIntensity("Intencity", Range(0, 10)) = 1
        _Size ("Size", Range(0, 10)) = 0.5
        _PositionTex("Position Tex", 2D) = "white" {}
        _PositionTex2("Position Tex 2", 2D) = "white" {}
        _PositionRatio("Position Ratio", Range(0, 1)) = 0
        _OffsetTex("Offset Tex", 2D) = "black" {}
        _NoiseTex("Noise Tex", 2D) = "white" {}
        _ColorPalleteTex("Color Pallete", 2D) = "white" {}
        _VolumeDeltaHeight("Volume Delta Height", Float) = 0
        _AlphaRandomWeight("Alpha Random Weight", Float) = 0.6
        [Toggle] _Reflection("Reflection", Float) = 0
        _PlanarReflectionY("Planar Reflection Y", Float) = 0
        _SizeOverHeightLower("Size Over Height Lower", Float) = 200
        _SizeOverHeightUpper("Size Over Height Upper", Float) = 500
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
                float _PositionRatio;
                float _PlanarReflectionY;
                float _Reflection;
                float _SizeOverHeightLower;
                float _SizeOverHeightUpper;

                SamplerState sampler_SpriteTex;

                sampler2D _PositionTex;
                sampler2D _PositionTex2;
                sampler2D _OffsetTex;
                sampler2D _NoiseTex;
                float4 _NoiseTex_ST;
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
                    // float4 pos2 = tex2Dlod(_PositionTex2, lodCoord);
                    // 1pos = lerp(pos, pos2, _PositionRatio);
                    float4 offset = tex2Dlod(_OffsetTex, lodCoord);
                    output.pos = float4(pos.xyz + offset.xyz, 1);

                    // Apply volume change

                    output.pos.y += _VolumeDeltaHeight * (max(0, pos.y - 80) / (250 - 80));

                    output.pos = mul(unity_ObjectToWorld, output.pos);

                    output.normal = v.normal;
                    output.tex0 = v.texcoord;

                    // Light effects

                    float4 noiseCoord = float4(v.texcoord * _NoiseTex_ST.xy + _NoiseTex_ST.zw, 0, 0);
                    float4 noise = tex2Dlod(_NoiseTex, noiseCoord);
                    float lightNoise = noise.r;
                    float phase = noise.g;

                    float intense = clamp(sin(_Time.y + phase * 50) * 1.5 + 0.5, 0, 1);
                    float minIntenseOverHeight = lerp(0.5, 1, saturate((pos.y - _SizeOverHeightLower) / (_SizeOverHeightUpper - _SizeOverHeightLower)));
                    // float intenseOverHeight = 1 + clamp((pos.y - _SizeOverHeightLower) / (_SizeOverHeightUpper - _SizeOverHeightLower), 0, 1);
                    intense = max(minIntenseOverHeight, intense);

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
                [maxvertexcount(8)]
                void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
                {
                    float4 lodCoord = float4(p[0].tex0, 0, 0);
                    float4 noiseCoord = float4(lodCoord.xy * _NoiseTex_ST.xy + _NoiseTex_ST.zw, 0, 0);
                    float4 noise = tex2Dlod(_NoiseTex, noiseCoord);

                    float3 look = _WorldSpaceCameraPos - p[0].pos;
                    look = normalize(look);
                    float3 right = cross(look, float3(0, 1, 0));
                    right = normalize(right);
                    float3 up = cross(look, right);
                    up = normalize(up);

                    float minScale = saturate((p[0].pos.y - _SizeOverHeightLower) / (_SizeOverHeightUpper - _SizeOverHeightLower)) * 0.7;
                    float scale = max(minScale, noise.b);
                    float halfS = 0.5f * _Size * scale * 1;
                            
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

                    float texEdge = _Reflection ? 0.5f : 1.0f;

                    FS_INPUT pIn;
                    pIn.pos = mul(vp, v[0]);
                    pIn.tex0 = float2(texEdge, 0.0f);
                    pIn.color = p[0].color;
                    triStream.Append(pIn);

                    pIn.pos =  mul(vp, v[1]);
                    pIn.tex0 = float2(texEdge, 1.0f);
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

                    // Reflection
                    if (_Reflection && p[0].pos.y > _PlanarReflectionY)
                    {
                        triStream.RestartStrip();

                        float4 refl = p[0].pos;
                        refl.y = _PlanarReflectionY * 2 - refl.y;

                        look = _WorldSpaceCameraPos - refl;
                        look = normalize(look);
                        right = cross(look, float3(0, 1, 0));
                        right = normalize(right);
                        up = cross(look, right);
                        up = normalize(up);

                        halfS *= max(1, min(100, 0.3 * pow(p[0].pos.y - refl.y, 0.5)));
                        p[0].color.a *= min(0.6f, 50 * pow(p[0].pos.y - refl.y, -1.3));
                        v[0] = float4(refl + halfS * right - halfS * up, 1.0f);
                        v[1] = float4(refl + halfS * right + halfS * up, 1.0f);
                        v[2] = float4(refl - halfS * right - halfS * up, 1.0f);
                        v[3] = float4(refl - halfS * right + halfS * up, 1.0f);

                        pIn.pos = mul(vp, v[0]);
                        pIn.tex0 = float2(1.0f, 0.0f);
                        pIn.color = p[0].color;
                        triStream.Append(pIn);

                        pIn.pos = mul(vp, v[1]);
                        pIn.tex0 = float2(1.0f, 1.0f);
                        pIn.color = p[0].color;
                        triStream.Append(pIn);

                        pIn.pos = mul(vp, v[2]);
                        pIn.tex0 = float2(0.5f, 0.0f);
                        pIn.color = p[0].color;
                        triStream.Append(pIn);

                        pIn.pos = mul(vp, v[3]);
                        pIn.tex0 = float2(0.5f, 1.0f);
                        pIn.color = p[0].color;
                        triStream.Append(pIn);
                    }
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
