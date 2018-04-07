using System.Collections;
using System.Collections.Generic;
using ParticleCities;
using UnityEngine;

public class SimpleFlowMotion : ParticleMotionBase {
    
    private Vector4 shaderVector = new Vector4(0, 0, 0, 1);

    private ObjectTrailing leftHand;
    private ObjectTrailing rightHand;

    protected override void UpdateInput()
    {
        Transform leftHandObj = InputManager.Instance.GetHand(HandType.Left);
        leftHand = leftHandObj == null ? null : leftHandObj.GetComponent<ObjectTrailing>();

        Transform rightHandObj = InputManager.Instance.GetHand(HandType.Right);
        rightHand = rightHandObj == null ? null : rightHandObj.GetComponent<ObjectTrailing>();

        for (int i = 0; i < ObjectTrailing.TrailLength; i++)
        {
            if (leftHand != null && leftHand.Frames != null)
            {
                shaderVector.x = leftHand.Frames[i].Position.x;
                shaderVector.y = leftHand.Frames[i].Position.y;
                shaderVector.z = leftHand.Frames[i].Position.z;
                particleMotionBlitMaterial.SetVector("_LeftHandPos" + i, shaderVector);

                shaderVector.x = leftHand.Frames[i].Velocity.x;
                shaderVector.y = leftHand.Frames[i].Velocity.y;
                shaderVector.z = leftHand.Frames[i].Velocity.z;
                particleMotionBlitMaterial.SetVector("_LeftHandVel" + i, shaderVector);
            }

            if (rightHand != null && rightHand.Frames != null)
            {
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
}
