using ParticleCities;
using UnityEngine;
using WanderUtils;

public class SimpleFlowMotion : ParticleMotionBase
{
    [Header("Debug")] 
    public bool Visualize;

    private Vector4 shaderVector = new Vector4(0, 0, 0, 1);

    private ObjectTrailing leftHand;
    private ObjectTrailing rightHand;
    private ObjectTrailing activeObject;

    private ParticleCityPlayerController playerController;

    private static float FlyingPushRatio = 0.15f;

    public override void Start()
    {
        base.Start();

        playerController = FindObjectOfType<ParticleCityPlayerController>();
    }

    protected override void UpdateInput()
    {
        Transform leftHandObj = InputManager.Instance.GetHand(HandType.Left);
        leftHand = leftHandObj == null ? null : leftHandObj.GetComponent<ObjectTrailing>();

        Transform rightHandObj = InputManager.Instance.GetHand(HandType.Right);
        rightHand = rightHandObj == null ? null : rightHandObj.GetComponent<ObjectTrailing>();

        // activeObject = null;
        // for (int i = 0; i < ParticleCity.Instance.ActiveGameObjects.Count; i++)
        // {
        //     ObjectTrailing[] objectTrailings = ParticleCity.Instance.ActiveGameObjects[i].GetComponents<ObjectTrailing>();
        //     if (objectTrailings.Length > 0)
        //     {
        //         activeObject = objectTrailings[0];
        //         break;
        //     }
        // }

        Vector3 flyingPush = playerController != null ? playerController.CurrentVelocity * FlyingPushRatio : Vector3.zero;

        for (int i = 0; i < ObjectTrailing.TrailLength; i++)
        {
            if (leftHand != null && leftHand.Frames != null)
            {
                Vector3 localPos = transform.InverseTransformPoint(leftHand.Frames[i].Position);
                shaderVector.x = localPos.x + flyingPush.x;
                shaderVector.y = localPos.y + flyingPush.y;
                shaderVector.z = localPos.z + flyingPush.z;
                particleMotionBlitMaterial.SetVector("_LeftHandPos" + i, shaderVector);

                Vector3 localVel = transform.InverseTransformVector(leftHand.Frames[i].Velocity);
                shaderVector.x = localVel.x;
                shaderVector.y = localVel.y;
                shaderVector.z = localVel.z;
                particleMotionBlitMaterial.SetVector("_LeftHandVel" + i, shaderVector);
            }

            if (rightHand != null && rightHand.Frames != null)
            {
                Vector3 localPos = transform.InverseTransformPoint(rightHand.Frames[i].Position);
                shaderVector.x = localPos.x + flyingPush.x;
                shaderVector.y = localPos.y + flyingPush.y;
                shaderVector.z = localPos.z + flyingPush.z;
                particleMotionBlitMaterial.SetVector("_RightHandPos" + i, shaderVector);

                Vector3 localVel = transform.InverseTransformVector(rightHand.Frames[i].Velocity);
                shaderVector.x = localVel.x;
                shaderVector.y = localVel.y;
                shaderVector.z = localVel.z;
                particleMotionBlitMaterial.SetVector("_RightHandVel" + i, shaderVector);
            }

            // if (activeObject != null && activeObject.Frames != null && activeObject.Frames.Length > 0)
            // {
            //     shaderVector.x = activeObject.Frames[i].Position.x;
            //     shaderVector.y = activeObject.Frames[i].Position.y;
            //     shaderVector.z = activeObject.Frames[i].Position.z;
            //     particleMotionBlitMaterial.SetVector("_ActiveObjectPos" + i, shaderVector);

            //     shaderVector.x = activeObject.Frames[i].Velocity.x;
            //     shaderVector.y = activeObject.Frames[i].Velocity.y;
            //     shaderVector.z = activeObject.Frames[i].Velocity.z;
            //     particleMotionBlitMaterial.SetVector("_ActiveObjectVel" + i, shaderVector);
            // }
            // else
            // {
            //     shaderVector = new Vector4(0, 0, 0, 1);
            //     particleMotionBlitMaterial.SetVector("_ActiveObjectVel" + i, shaderVector);
            // }
        }

        int objectIndex = 0;
        for (int i = 0; i < ParticleCity.Current.ActiveGameObjects.Count; i++)
        {
            if (ParticleCity.Current.ActiveGameObjects[i] == null)
            {
                continue;
            }

            ObjectTrailing[] objectTrailings = ParticleCity.Current.ActiveGameObjects[i].GetComponents<ObjectTrailing>();
            if (objectTrailings.Length > 0 && objectTrailings[0].Frames != null && objectTrailings[0].Frames.Length > 0)
            {
                ObjectTrailing.TrailFrame frame = objectTrailings[0].Frames[0];
                Vector3 localPos = transform.InverseTransformPoint(frame.Position);
                shaderVector.x = localPos.x;
                shaderVector.y = localPos.y;
                shaderVector.z = localPos.z;
                particleMotionBlitMaterial.SetVector("_ActiveObjectPos" + objectIndex, shaderVector);

                Vector3 localVel = transform.InverseTransformVector(frame.Velocity);
                shaderVector.x = localVel.x;
                shaderVector.y = localVel.y;
                shaderVector.z = localVel.z;
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

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || !Visualize)
        {
            return;
        }

        Vector3 flyingPush = playerController.CurrentVelocity * FlyingPushRatio;
        Gizmos.color = Color.white;

        for (int i = 0; i < ObjectTrailing.TrailLength; i++)
        {
            if (leftHand != null && leftHand.Frames != null)
            {
                Gizmos.DrawWireSphere(leftHand.Frames[i].Position + flyingPush, 0.2f);
                Gizmos.DrawRay(leftHand.Frames[i].Position + flyingPush, -leftHand.Frames[i].Velocity / 50);
            }

            if (rightHand != null && rightHand.Frames != null)
            {
                Gizmos.DrawWireSphere(rightHand.Frames[i].Position + flyingPush, 0.2f);
                Gizmos.DrawRay(rightHand.Frames[i].Position + flyingPush, -rightHand.Frames[i].Velocity / 50);
            }
        }

        for (int i = 0; i < ParticleCity.Current.ActiveGameObjects.Count; i++)
        {
            if (ParticleCity.Current.ActiveGameObjects[i] == null)
            {
                continue;
            }

            int objectIndex = 0;
            ObjectTrailing[] objectTrailings = ParticleCity.Current.ActiveGameObjects[i].GetComponents<ObjectTrailing>();
            if (objectTrailings.Length > 0 && objectTrailings[0].Frames != null && objectTrailings[0].Frames.Length > 0)
            {
                ObjectTrailing.TrailFrame frame = objectTrailings[0].Frames[0];
                Gizmos.DrawWireSphere(frame.Position, 0.2f);
                Gizmos.DrawRay(frame.Position, -frame.Velocity / 50);

                objectIndex++;
                if (objectIndex >= ObjectTrailing.TrailLength)
                {
                    break;
                }
            }
        }
    }
}
