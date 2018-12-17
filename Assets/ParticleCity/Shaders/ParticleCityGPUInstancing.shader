Shader "Particle City/Particle City GPU Instancing" 
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
        _MinHeight("MinHeight", Float) = -10000
        _MaxHeight("MaxHeight", Float) = 10000
        _InstanceRowOffset("Instancing Row Offset", Float) = 0
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
            // Blend One OneMinusSrcAlpha
            // color = 1 * src + (1 - src.a) * dst
            Blend SrcAlpha One
            // color = src.a * src + 1 * dst
        
            CGPROGRAM
                #pragma target 4.5
                #pragma vertex VS_Main
                #pragma fragment FS_Main
                #pragma multi_compile_instancing
                #include "UnityCG.cginc" 

                // **************************************************************
                // Data structures                                                *
                // **************************************************************
                struct VS_INPUT {
                    float4 vertex   : POSITION;
                    float2 uvPoint  : TEXCOORD0;
                    float2 uvSprite : TEXCOORD1;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct FS_INPUT
                {
                    float4  pos      : POSITION;
                    float4  color    : COLOR0;
                    float2  uvSprite : TEXCOORD0;
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

                float _MinHeight;
                float _MaxHeight;

                SamplerState sampler_SpriteTex;

                sampler2D _PositionTex;
                sampler2D _PositionTex2;
                sampler2D _OffsetTex;
                sampler2D _NoiseTex;
                float4 _NoiseTex_ST;
                sampler2D _ColorPalleteTex;

                UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _InstancingRowOffset)
                UNITY_INSTANCING_BUFFER_END(Props)

                // **************************************************************
                // Shader Programs                                                *
                // **************************************************************

                float4 expandToQuad(float4 worldPos, float4 rawPos, float2 uvPoint, float2 uvSprite)
                {
                    float4 noiseCoord = float4(uvPoint * _NoiseTex_ST.xy + _NoiseTex_ST.zw, 0, 0);
                    float4 noise = tex2Dlod(_NoiseTex, noiseCoord);

                    float3 look = _WorldSpaceCameraPos - worldPos;
                    look = normalize(look);
                    float3 right = cross(look, float3(0, 1, 0));
                    right = normalize(right);
                    float3 up = cross(look, right);
                    up = normalize(up);

                    float minScale = saturate((rawPos.y - _SizeOverHeightLower) / (_SizeOverHeightUpper - _SizeOverHeightLower)) * 0.7;
                    float scale = max(minScale, noise.b);
                    float modelScale = length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x)); // scale x axis
                    float halfS = 0.5f * _Size * scale * modelScale * 1;

                    //              0       1         2       3
                    // uvSprite: (0, 0)   (1, 0)   (1, 1)   (0, 1)
                    //     *2-1: (-1, -1) (1, -1)  (1, 1)   (-1, 1)
                    //       
                    //       GS: (1, -1)  (1, 1)  (-1, -1)  (-1, 1)
                    float2 uvOffset = uvSprite * 2 - 1;

                    float4 newPos = float4(worldPos + uvOffset.x * halfS * right + uvOffset.y * halfS * up, 1.0f);


#if UNITY_VERSION >= 560 
                    float4x4 vp = mul(UNITY_MATRIX_MVP, unity_WorldToObject);
#else 
#if UNITY_SHADER_NO_UPGRADE 
                    float4x4 vp = mul(UNITY_MATRIX_MVP, unity_WorldToObject);
#endif 
#endif
                    newPos = mul(vp, newPos);

                    return newPos;
                }

                // Vertex Shader ------------------------------------------------
                FS_INPUT VS_Main(VS_INPUT v)
                {
                    UNITY_SETUP_INSTANCE_ID(v);

                    FS_INPUT output = (FS_INPUT)0;

                    float rowOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _InstancingRowOffset);
                    float4 lodCoord = float4(v.uvPoint.x, v.uvPoint.y + rowOffset, 0, 0);
                    
                    float4 pos = tex2Dlod(_PositionTex, lodCoord);
                    // float4 pos2 = tex2Dlod(_PositionTex2, lodCoord);
                    // pos = lerp(pos, pos2, _PositionRatio);
                    float4 offset = tex2Dlod(_OffsetTex, lodCoord);
                    output.pos = float4(pos.xyz + offset.xyz, 1);

                    // Apply volume change

                    output.pos.y += _VolumeDeltaHeight * (max(0, pos.y - 80) / (250 - 80));
                    output.pos = mul(unity_ObjectToWorld, output.pos);
                    output.pos = expandToQuad(output.pos, pos, lodCoord.xy, v.uvSprite);
                    output.uvSprite = v.uvSprite;

                    if (pos.y < _MinHeight || pos.y > _MaxHeight)
                    {
                        output.color = float4(0, 0, 0, 0);
                        return output;
                    }

                    // Light effects

                    float4 noiseCoord = float4(lodCoord.xy * _NoiseTex_ST.xy + _NoiseTex_ST.zw, 0, 0);
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

                    if (length(output.pos) < 5)
                    {
                        output.color = float4(0, 0, 0, 0);
                    }

                    // Fog
                    // float fog = 1 - 0.7 * saturate((distance(_WorldSpaceCameraPos, output.pos) - 100) / (1000 - 100));
                    // output.color *= (fog * 0.2 + 1);
                    // output.color.a *= 1 - 0.7 * saturate((distance(_WorldSpaceCameraPos, output.pos) - 100) / (1000 - 100));

                    return output;
                }

                // Fragment Shader -----------------------------------------------
                float4 FS_Main(FS_INPUT input) : COLOR
                {
                    float4 c = _SpriteTex.Sample(sampler_SpriteTex, input.uvSprite);
                    c *= input.color;

#if !defined(UNITY_COLORSPACE_GAMMA)
                    c.a = pow(c.a, 2.2);
#endif
                    // c.rgb *= c.a;
                    c *= 2;

                    return c;
                }

            ENDCG
        }
    } 
}
