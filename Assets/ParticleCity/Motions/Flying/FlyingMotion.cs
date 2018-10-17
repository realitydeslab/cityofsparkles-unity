using System.Collections;
using System.Collections.Generic;
using ParticleCities;
using UnityEngine;
using WanderUtils;

public class FlyingMotion : ParticleMotionBase {
    
    private Vector4 shaderVector = new Vector4(0, 0, 0, 1);

    private ObjectTrailing leftHand;
    private ObjectTrailing rightHand;

    public override void Start()
    {
        base.Start();

        leftHand = InputManager.Instance.GetHand(HandType.Left).GetComponent<ObjectTrailing>();
        rightHand = InputManager.Instance.GetHand(HandType.Right).GetComponent<ObjectTrailing>();
    }

    protected override void UpdateInput()
    {
        for (int i = 0; i < ObjectTrailing.TrailLength; i++)
        {
            shaderVector.x = leftHand.Frames[i].Position.x;
            shaderVector.y = leftHand.Frames[i].Position.y;
            shaderVector.z = leftHand.Frames[i].Position.z;
            particleMotionBlitMaterial.SetVector("_LeftHandPos" + i, shaderVector);

            shaderVector.x = leftHand.Frames[i].Velocity.x;
            shaderVector.y = leftHand.Frames[i].Velocity.y;
            shaderVector.z = leftHand.Frames[i].Velocity.z;
            particleMotionBlitMaterial.SetVector("_LeftHandVel" + i, shaderVector);

            shaderVector.x = rightHand.Frames[i].Position.x;
            shaderVector.y = rightHand.Frames[i].Position.y;
            shaderVector.z = rightHand.Frames[i].Position.z;
            particleMotionBlitMaterial.SetVector("_RightHandPos" + i, shaderVector);

            shaderVector.x = rightHand.Frames[i].Velocity.x;
            shaderVector.y = rightHand.Frames[i].Velocity.y;
            shaderVector.z = rightHand.Frames[i].Velocity.z;
            particleMotionBlitMaterial.SetVector("_RightHandVel" + i, shaderVector);
        }
    }
}
