Shader "Particle City/ParticleMotionBlit" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BasePositionTex("Base Position Tex", 2D) = "white" {}

        // Set for velocity pass
        _OffsetTex("Offset Tex", 2D) = "black" {}

        // Set for offset pass
        _VelocityTex("Velocity Tex", 2D) = "black" {}

        _RightHandPos("Right Hand Position", Vector) = (0, 0, 0, 1)
        _LeftHandPos("Right Hand Position", Vector) = (0, 0, 0, 1)

        _SpringDrag("Spring Drag", Float) = 1
        _SpringDamp("Spring Damp", Float) = 0.5
        _HandPush("Hand Push", Float) = 1
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D _BasePositionTex;
    sampler2D _VelocityTex;
    sampler2D _OffsetTex;

    float4 _RightHandPos;
    float4 _LeftHandPos;

    float _SpringDrag;
    float _SpringDamp;
    float _HandPush;

    float4 frag_init(v2f_img i) : SV_Target{
        // Init offset or speed by 0
        return float4(0, 0, 0, 1);
    }

    float4 frag_velocity_update(v2f_img i) : SV_Target{
        float3 base = tex2D(_BasePositionTex, i.uv);
        float3 offset = tex2D(_OffsetTex, i.uv);
        float3 velocity = tex2D(_MainTex, i.uv);

        // TODO: Optimize

        // Right hand
        float3 rightHandToPoint = base + offset - _RightHandPos;
        float3 rightHandToPointLength = length(rightHandToPoint);
        float3 rightHandPushAcc = _HandPush * pow(rightHandToPointLength, -3) * rightHandToPoint - _SpringDamp * velocity;

        // Left hand
        float3 leftHandToPoint = base + offset - _LeftHandPos;
        float3 leftHandToPointLength = length(leftHandToPoint);
        float3 leftHandPushAcc = _HandPush * pow(leftHandToPointLength, -3) * leftHandToPoint - _SpringDamp * velocity;

        float3 springDragAcc = _SpringDrag * -offset;

        float3 acc = springDragAcc + rightHandPushAcc + leftHandPushAcc;
        velocity += acc * unity_DeltaTime.x;

        return float4(velocity, 1);

        // Follow hand
        /*
        float3 direction = normalize(_RightHandPos.xyz - (base + offset));
        return float4(direction * 5, 1);
        */
    }

    float4 frag_offset_update(v2f_img i) : SV_Target{
        float3 offset = tex2D(_MainTex, i.uv);
        float3 velocity = tex2D(_VelocityTex, i.uv);

        offset += velocity * unity_DeltaTime.x;

        return float4(offset, 1);

        // Follow hand
        /*
        offset.xyz += velocity.xyz * unity_DeltaTime.x;
        return offset;
        */
    }

    ENDCG

    SubShader {
        // Pass 0: Init
        Pass {
            CGPROGRAM

            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_init

            ENDCG
        }

        // Pass 1: Velocity Update
        Pass {
            CGPROGRAM

            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_velocity_update

            ENDCG
        }

        // Pass 2: Offset Update
        Pass {
            CGPROGRAM

            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_offset_update

            ENDCG
        }
    }
}
