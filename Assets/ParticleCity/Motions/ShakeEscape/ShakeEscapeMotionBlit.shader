Shader "Particle City/ShakeEscapeMotionBlit" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BasePositionTex("Base Position Tex", 2D) = "white" {}

        // Set for velocity pass
        _OffsetTex("Offset Tex", 2D) = "black" {}

        // Set for offset pass
        _VelocityTex("Velocity Tex", 2D) = "black" {}

        _ImpulseNoise("Impulse Noise", 2D) = "black" {}
        _ImpulseScale("Impulse Scale", Float) = 0
        _VerticalImpulse("Vertical Pulse", Float) = 0

        _HandGravityScale("Hand Gravity Scale", Float) = 0

        _TwistSpeed("Twist Speed", Float) = 0
        _TwistForce("Twist Force", Float) = 0

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

    sampler2D _ImpulseNoise;
    float _ImpulseScale;
    float _VerticalImpulse;

    float4 _RightHandPos;
    float4 _LeftHandPos;
    float4 _HeadPos;
    float4 _ActiveObjectPos0;
    float4 _ActiveObjectPos1;
    float4 _ActiveObjectPos2;
    float4 _ActiveObjectPos3;
    float4 _ActiveObjectPos4;
    float4 _ActiveObjectPos5;
    float4 _ActiveObjectPos6;
    float4 _ActiveObjectPos7;

    float _LeftHandGravity;
    float _RightHandGravity;
    float _HandGravityScale;

    float4 _LeftHandForward;
    float4 _RightHandForward;

    float _TwistSpeed;
    float _TwistForce;

    float _SpringDrag;
    float _SpringDamp;
    float _HandPush;

    float4 frag_init(v2f_img i) : SV_Target{
        // Init speed 
        return float4(0, 0, 0, 1);
    }

    float applySimplePush(float3 sourcePos, float3 base, float3 offset)
    {
        float3 sourceToPoint = base + offset - sourcePos;
        float sourceToPointLength = length(sourceToPoint);
        float3 sourcePushAcc = _HandPush * pow(sourceToPointLength, -3) * sourceToPoint;
        
        return sourcePushAcc;
    }

    float3 applyPush(float3 sourcePos, float3 forward, float3 base, float3 offset, float gravity)
    {
        float3 sourceToPoint = base + offset - sourcePos;
        float sourceToPointLength = length(sourceToPoint);
        float3 sourcePushAcc = _HandPush * (1 - pow(gravity, 2)) * pow(sourceToPointLength, -3) * sourceToPoint;
        
        // Gravity
        float3 sourceGravityAcc = _HandGravityScale * pow(sourceToPointLength, -2) * -sourceToPoint * gravity;

        // Twist
        float3 sourceToPointProjectedOnForward = dot(sourceToPoint, forward) * forward;
        float3 pointPerpendicularToForward = sourceToPointProjectedOnForward - sourceToPoint;
        float3 tangent = cross(forward, normalize(pointPerpendicularToForward));
        
        float splitDist = 1;
        // float3 distFactor = 1 / sourceToPointLength * max(0, sign(splitDist - sourceToPointLength))
        //                    + (1 / splitDist - pow(splitDist, -2) + pow(sourceToPointLength, -2)) * max(0, sign(sourceToPointLength - splitDist));
        // float3 distFactor = pow(min(sourceToPointLength, 100), -2); 
        float distFactor = 0;
        if (sourceToPointLength < 3000)
        {
            distFactor = 1 / sourceToPointLength;
        }
        float3 twistAcc = tangent * _TwistForce * gravity * distFactor;

        return sourcePushAcc + sourceGravityAcc + twistAcc;
    }

    float4 frag_velocity_update(v2f_img i) : SV_Target{
        float3 base = tex2D(_BasePositionTex, i.uv);
        float3 offset = tex2D(_OffsetTex, i.uv);
        float3 velocity = tex2D(_MainTex, i.uv);

        float gravityFactor = 1 - min(1, _RightHandGravity + _LeftHandGravity);

        float3 damping = _SpringDamp;
        float3 drag = _SpringDrag * gravityFactor;

        // TODO: Use random impulse location
        // TODO: Deal with time correctly
        // impulse
        float3 impulseNoise = tex2D(_ImpulseNoise, i.uv * fmod(_Time * 1000, 1000));
        float3 headToPoint = base + offset - _HeadPos;
        float3 pointDist = max(0, length(headToPoint) - 500);
        float impulse = (_ImpulseScale * gravityFactor) / (1 + log(1 + pointDist));
        velocity += impulse * impulseNoise;

        // hard-coded based on current city height
        // velocity += _VerticalImpulse * impulseNoise * (max(0, (base + offset).y - 80) / (250 - 80)) * _Time;

        // Push
        float3 acc = float3(0, 0, 0);
        acc += applyPush(_RightHandPos, _RightHandForward, base, offset, _RightHandGravity);
        acc += applyPush(_LeftHandPos, _LeftHandForward, base, offset, _LeftHandGravity);
        acc += applySimplePush(_HeadPos, base, offset);
        acc += applySimplePush(_ActiveObjectPos0, base, offset);
        acc += applySimplePush(_ActiveObjectPos1, base, offset);
        acc += applySimplePush(_ActiveObjectPos2, base, offset);
        acc += applySimplePush(_ActiveObjectPos3, base, offset);
        acc += applySimplePush(_ActiveObjectPos4, base, offset);
        acc += applySimplePush(_ActiveObjectPos5, base, offset);
        acc += applySimplePush(_ActiveObjectPos6, base, offset);
        acc += applySimplePush(_ActiveObjectPos7, base, offset);

        // Damping spring
        float3 springDragAcc = drag * -offset;
        float3 dampingAcc = -damping * velocity;

        acc += springDragAcc + dampingAcc;
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
