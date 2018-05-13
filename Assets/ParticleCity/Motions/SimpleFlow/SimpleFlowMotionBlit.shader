Shader "Particle City/SimpleFlowMotionBlit" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BasePositionTex("Base Position Tex", 2D) = "white" {}

        // Set for velocity pass
        _OffsetTex("Offset Tex", 2D) = "black" {}

        // Set for offset pass
        _VelocityTex("Velocity Tex", 2D) = "black" {}

        _SpringDrag("Spring Drag", Float) = 1
        _SpringDamp("Spring Damp", Float) = 0.5
        _ClosingDamp("Closing Damp", Float) = 0.5
        _HandPush("Hand Push", Float) = 1
        _MaxDistance("Max Distance", Float) = 100

        _VanR_0("Van R_0", Float) = 100
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D _BasePositionTex;
    sampler2D _VelocityTex;
    sampler2D _OffsetTex;

    float _SpringDrag;
    float _SpringDamp;
    float _ClosingDamp;
    float _HandPush;
    float _MaxDistance;
    float _VanR_0;

    float4 _ActiveObjectPos0;
    float4 _ActiveObjectPos1;
    float4 _ActiveObjectPos2;
    float4 _ActiveObjectPos3;
    float4 _ActiveObjectPos4;
    float4 _ActiveObjectPos5;
    float4 _ActiveObjectPos6;
    float4 _ActiveObjectPos7;

    float4 _ActiveObjectVel0;
    float4 _ActiveObjectVel1;
    float4 _ActiveObjectVel2;
    float4 _ActiveObjectVel3;
    float4 _ActiveObjectVel4;
    float4 _ActiveObjectVel5;
    float4 _ActiveObjectVel6;
    float4 _ActiveObjectVel7;

    float4 _LeftHandPos0;
    float4 _LeftHandPos1;
    float4 _LeftHandPos2;
    float4 _LeftHandPos3;
    float4 _LeftHandPos4;
    float4 _LeftHandPos5;
    float4 _LeftHandPos6;
    float4 _LeftHandPos7;

    float4 _LeftHandVel0;
    float4 _LeftHandVel1;
    float4 _LeftHandVel2;
    float4 _LeftHandVel3;
    float4 _LeftHandVel4;
    float4 _LeftHandVel5;
    float4 _LeftHandVel6;
    float4 _LeftHandVel7;

    float4 _RightHandPos0;
    float4 _RightHandPos1;
    float4 _RightHandPos2;
    float4 _RightHandPos3;
    float4 _RightHandPos4;
    float4 _RightHandPos5;
    float4 _RightHandPos6;
    float4 _RightHandPos7;

    float4 _RightHandVel0;
    float4 _RightHandVel1;
    float4 _RightHandVel2;
    float4 _RightHandVel3;
    float4 _RightHandVel4;
    float4 _RightHandVel5;
    float4 _RightHandVel6;
    float4 _RightHandVel7;

    float3 apply_input(float3 base, float3 offset, float3 velocity, float3 inputPos, float3 inputVel, float weight)
    {
        float3 pointToInput = inputPos - (base + offset);
        float3 direction = normalize(pointToInput);
        float distance = length(pointToInput);

        // Simple follow 3
        /*
        float adjustedStrength = max(0, dot(direction, inputVel));
        float3 acc = _HandPush * direction * pow(distance, -2) * adjustedStrength;
        */

        // Spring with max distance
        // float dirAdjust = max(0, sign(_MaxDistance - distance)) * max(0, dot(direction, inputVel));
        /*
        if (distance > _MaxDistance)
        {
            return float3(0, 0, 0);
        }
        float inputVelLength = length(inputVel);
        if (inputVelLength < 100)
        {
            return float3(0, 0, 0);
        }
        float inputVelDot = dot(direction, inputVel);
        float inputCos = inputVelDot;
        if (inputCos < 0.9 * inputVelLength)
        {
            return float3(0, 0, 0);
        }
        float3 acc = _HandPush * direction * inputVelDot * pow(distance, 1);
        */

        /*
        // Simple follow
        float adjustedStrength = max(0, dot(direction, inputVel));
        float3 acc = _HandPush * direction * pow(distance, -2) * adjustedStrength;
        */

        // Trail force
        /*
        float adjustedForce = max(0, dot(direction, inputVel));
        float3 acc = _HandPush * direction * pow(distance, -2) * adjustedForce;
        */

        // Trail force split
        float adjustedForce = max(0, dot(direction, inputVel));
        float3 acc = _HandPush * weight * direction * pow(max(distance, _VanR_0), -2) * adjustedForce;
        float newVelocity = velocity + acc * unity_DeltaTime.x;

        if (distance < _VanR_0)
        {
            // Additional damping
            acc += -_ClosingDamp * newVelocity;
        }

        // Trail force by Van der Waals 
        /*
        // float vanForce = - (pow(_VanR_0 / distance, 12) - 2 * pow(_VanR_0 / distance, 6));
        float vanForce = - (pow(_VanR_0 / distance, 8) - 100 * pow(_VanR_0 / distance, 4));
        float adjustedForce = max(0, sign(dot(direction, inputVel)));
        // float adjustedForce = 1;
        float3 acc = _HandPush * direction * pow(distance, -2) * vanForce * adjustedForce;
        */

        return acc;
    }

    float4 frag_init(v2f_img i) : SV_Target{
        // Init offset or speed by 0
        return float4(0, 0, 0, 1);
    }

    float4 frag_velocity_update(v2f_img i) : SV_Target{
        float3 base = tex2D(_BasePositionTex, i.uv);
        float3 offset = tex2D(_OffsetTex, i.uv);
        float3 velocity = tex2D(_MainTex, i.uv);

        float3 acc = float3(0, 0, 0);
        acc += -_SpringDamp * velocity;
        acc += _SpringDrag * -offset;

        acc += apply_input(base, offset, velocity, _ActiveObjectPos0.xyz, _ActiveObjectVel0.xyz, 1.000);
        acc += apply_input(base, offset, velocity, _ActiveObjectPos1.xyz, _ActiveObjectVel1.xyz, 1.000);
        acc += apply_input(base, offset, velocity, _ActiveObjectPos2.xyz, _ActiveObjectVel2.xyz, 1.000);
        acc += apply_input(base, offset, velocity, _ActiveObjectPos3.xyz, _ActiveObjectVel3.xyz, 1.000);
        acc += apply_input(base, offset, velocity, _ActiveObjectPos4.xyz, _ActiveObjectVel4.xyz, 1.000);
        acc += apply_input(base, offset, velocity, _ActiveObjectPos5.xyz, _ActiveObjectVel5.xyz, 1.000);
        acc += apply_input(base, offset, velocity, _ActiveObjectPos6.xyz, _ActiveObjectVel6.xyz, 1.000);
        acc += apply_input(base, offset, velocity, _ActiveObjectPos7.xyz, _ActiveObjectVel7.xyz, 1.000);

        acc += apply_input(base, offset, velocity, _LeftHandPos0.xyz, _LeftHandVel0.xyz, 1.000);
        acc += apply_input(base, offset, velocity, _LeftHandPos1.xyz, _LeftHandVel1.xyz, 0.875);
        acc += apply_input(base, offset, velocity, _LeftHandPos2.xyz, _LeftHandVel2.xyz, 0.750);
        acc += apply_input(base, offset, velocity, _LeftHandPos3.xyz, _LeftHandVel3.xyz, 0.625);
        acc += apply_input(base, offset, velocity, _LeftHandPos4.xyz, _LeftHandVel4.xyz, 0.500);
        acc += apply_input(base, offset, velocity, _LeftHandPos5.xyz, _LeftHandVel5.xyz, 0.375);
        acc += apply_input(base, offset, velocity, _LeftHandPos6.xyz, _LeftHandVel6.xyz, 0.250);
        acc += apply_input(base, offset, velocity, _LeftHandPos7.xyz, _LeftHandVel7.xyz, 0.125);

        acc += apply_input(base, offset, velocity, _RightHandPos0.xyz, _RightHandVel0.xyz, 1.000);
        acc += apply_input(base, offset, velocity, _RightHandPos1.xyz, _RightHandVel1.xyz, 0.875);
        acc += apply_input(base, offset, velocity, _RightHandPos2.xyz, _RightHandVel2.xyz, 0.750);
        acc += apply_input(base, offset, velocity, _RightHandPos3.xyz, _RightHandVel3.xyz, 0.625);
        acc += apply_input(base, offset, velocity, _RightHandPos4.xyz, _RightHandVel4.xyz, 0.500);
        acc += apply_input(base, offset, velocity, _RightHandPos5.xyz, _RightHandVel5.xyz, 0.375);
        acc += apply_input(base, offset, velocity, _RightHandPos6.xyz, _RightHandVel6.xyz, 0.250);
        acc += apply_input(base, offset, velocity, _RightHandPos7.xyz, _RightHandVel7.xyz, 0.125);

        velocity += acc * unity_DeltaTime.x;
        return float4(velocity, 1);
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
