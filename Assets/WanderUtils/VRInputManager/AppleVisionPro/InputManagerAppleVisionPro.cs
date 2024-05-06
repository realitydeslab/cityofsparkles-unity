using UnityEngine;
using WanderUtils;
using CityOfSparkles.VisionOS;
using UnityEngine.XR.Hands;

namespace ParticleCities
{
    public class InputManagerAppleVisionPro : InputManager
    {
        [SerializeField] private Transform m_LeftHandJoint;

        [SerializeField] private Transform m_RightHandJoint;

        [SerializeField] private Transform m_XROrigin;

        private HandGestureManager m_HandGestureManager;

        public override Camera CenterCamera
        {
            get
            {
                //Debug.Log("[InputManagerAppleVisionPro] get CenterCamera");
                return Camera.main;
            }
        }

        public override Transform PlayerTransform
        {
            get
            {
                //Debug.Log("[InputManagerAppleVisionPro] get PlayerTransform");
                //return Camera.main.transform;
                return m_XROrigin;
            }
        }

        public override bool IsGrabContinuous
        {
            get => true;
        }

        public override bool HasSticker
        {
            get => false;
        }

        public override bool HasTouchpad
        {
            get => false;
        }

        public override bool GetButtonDown(Button button)
        {
            return false;
        }

        public override bool GetGrabDown(HandType handType)
        {
            return false;
        }

        public override bool GetGrabUp(HandType handType)
        {
            return false;
        }

        public override float GetGrabValue(HandType handType)
        {
            return 0f;
        }

        public override Transform GetHand(HandType handType)
        {
            switch (handType)
            {
                case HandType.Left:
                    return m_LeftHandJoint;
                case HandType.Right:
                    return m_RightHandJoint;
                default:
                    return null;
            }
        }

        public override HandType GetHandType(Transform transform)
        {
            return HandType.Unknown;
        }

        public override HandType GetLastActiveHand()
        {
            return HandType.Unknown;
        }

        public override Vector2 GetStickerValue(HandType handType)
        {
            return Vector2.zero;
        }

        public override Vector2 GetTouchpadValue(HandType handType, out bool isPressed)
        {
            isPressed = false;
            return Vector2.zero;
        }

        public override float GetTriggerValue(HandType handType)
        {
            if (m_HandGestureManager == null)
            {
                m_HandGestureManager = FindObjectOfType<HandGestureManager>();
                if (m_HandGestureManager == null)
                    return 0f;
            }

            Handedness handedness = handType == HandType.Left ? Handedness.Left : handType == HandType.Right ? Handedness.Right : Handedness.Invalid;
            if (m_HandGestureManager.HandGestures[handedness] == HandGesture.Pinching)
                return 1f;

            return 0f;
        }

        public override bool IsActiveHand(GameObject candidate)
        {
            return false;
        }

        public override bool IsDeviceIdle()
        {
            return true;
        }

        public override void SetControllerVisible(bool visible)
        {
            
        }
    }
}
