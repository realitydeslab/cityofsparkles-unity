using ParticleCities;
using UnityEngine;

public class FlyAwayMotion : ParticleMotionBase {
    
    private Vector4 shaderVector = new Vector4(0, 0, 0, 1);

    public ObjectTrailing LeftHand;
    public ObjectTrailing RightHand;
    public GameObject[] ActiveGameObjects;

    protected override void UpdateInput()
    {
        ObjectTrailing leftHand = LeftHand;
        ObjectTrailing rightHand = RightHand;

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

        int objectIndex = 0;
        for (int i = 0; i < ActiveGameObjects.Length; i++)
        {
            if (ActiveGameObjects[i] == null)
            {
                continue;
            }

            ObjectTrailing[] objectTrailings = ActiveGameObjects[i].GetComponents<ObjectTrailing>();
            if (objectTrailings.Length > 0 && objectTrailings[0].Frames != null && objectTrailings[0].Frames.Length > 0)
            {
                ObjectTrailing.TrailFrame frame = objectTrailings[0].Frames[0];
                shaderVector.x = frame.Position.x;
                shaderVector.y = frame.Position.y;
                shaderVector.z = frame.Position.z;
                particleMotionBlitMaterial.SetVector("_ActiveObjectPos" + objectIndex, shaderVector);

                shaderVector.x = frame.Velocity.x;
                shaderVector.y = frame.Velocity.y;
                shaderVector.z = frame.Velocity.z;
                particleMotionBlitMaterial.SetVector("_ActiveObjectVel" + objectIndex, shaderVector);

                objectIndex++;
                if (objectIndex >= ObjectTrailing.TrailLength)
                {
                    break;
                }
            }
        }

        for (; objectIndex < 8; objectIndex++)
        {
            shaderVector.x = 0;
            shaderVector.y = 0;
            shaderVector.z = 0;
            particleMotionBlitMaterial.SetVector("_ActiveObjectPos" + objectIndex, shaderVector);

            shaderVector.x = 0;
            shaderVector.y = 0;
            shaderVector.z = 0;
            particleMotionBlitMaterial.SetVector("_ActiveObjectVel" + objectIndex, shaderVector);
        }
    }
}
