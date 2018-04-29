using System;
using UnityEngine;

namespace ParticleCities.Audio
{
    [RequireComponent(typeof(AkAmbient))]
    [RequireComponent(typeof(BoxCollider))]
    public class GestureSoundGenerator : MonoBehaviour
    {
        private AkAmbient akAmbient;
        private BoxCollider boundsCollider;

        [Header("Debug")] 
        public Vector3 LeftHandRelativePos;
        public Vector3 RightHandRelativePos;
        public Vector3 LeftHandRotation;
        public Vector3 RightHandRotation;

        void Start()
        {
            akAmbient = GetComponent<AkAmbient>();
            boundsCollider = GetComponent<BoxCollider>();
        }

        void Update()
        {
            transform.position = InputManager.Instance.PlayerTransform.position;
            transform.rotation = InputManager.Instance.PlayerTransform.rotation;
            transform.localScale = InputManager.Instance.PlayerTransform.localScale;

            Transform leftHand = InputManager.Instance.GetHand(HandType.Left);
            LeftHandRelativePos = getRelativePosition(leftHand.position);
            AkSoundEngine.SetRTPCValue("LeftHandPosX", LeftHandRelativePos.x, akAmbient.gameObject);
            AkSoundEngine.SetRTPCValue("LeftHandPosY", LeftHandRelativePos.y, akAmbient.gameObject);
            AkSoundEngine.SetRTPCValue("LeftHandPosZ", LeftHandRelativePos.z, akAmbient.gameObject);
            LeftHandRotation = getEuler(leftHand.rotation);
            AkSoundEngine.SetRTPCValue("LeftHandRotX", LeftHandRotation.x, akAmbient.gameObject);
            AkSoundEngine.SetRTPCValue("LeftHandRotY", LeftHandRotation.y, akAmbient.gameObject);
            AkSoundEngine.SetRTPCValue("LeftHandRotZ", LeftHandRotation.z, akAmbient.gameObject);

            Transform rightHand = InputManager.Instance.GetHand(HandType.Right);
            RightHandRelativePos = getRelativePosition(rightHand.position);
            AkSoundEngine.SetRTPCValue("RightHandPosX", RightHandRelativePos.x, akAmbient.gameObject);
            AkSoundEngine.SetRTPCValue("RightHandPosY", RightHandRelativePos.y, akAmbient.gameObject);
            AkSoundEngine.SetRTPCValue("RightHandPosZ", RightHandRelativePos.z, akAmbient.gameObject);
            RightHandRotation = getEuler(rightHand.rotation);
            AkSoundEngine.SetRTPCValue("RightHandRotX", RightHandRotation.x, akAmbient.gameObject);
            AkSoundEngine.SetRTPCValue("RightHandRotY", RightHandRotation.y, akAmbient.gameObject);
            AkSoundEngine.SetRTPCValue("RightHandRotZ", RightHandRotation.z, akAmbient.gameObject);
        }

        private Vector3 getRelativePosition(Vector3 worldPosition)
        {
            Bounds b = boundsCollider.bounds;
            Vector3 offset = (worldPosition - b.center);
            Vector3 total = b.extents; 
            return new Vector3(
                Mathf.Abs(offset.x) / total.x,
                Mathf.Abs(offset.y) / total.y,
                Mathf.Abs(offset.z) / total.z
            );
        }

        private Vector3 getEuler(Quaternion quaternion)
        {
            Vector3 euler = quaternion.eulerAngles;
            for (int i = 0; i < 3; i++)
            {
                if (euler[i] < 0)
                {
                    euler[i] += 360;
                }
            }
            return euler;
        }
    }
}
