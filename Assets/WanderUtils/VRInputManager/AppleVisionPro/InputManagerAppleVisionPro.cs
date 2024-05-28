using UnityEngine;
using WanderUtils;
using CityOfSparkles.VisionOS;
using UnityEngine.XR.Hands;

namespace ParticleCities
{
    public class InputManagerAppleVisionPro : InputManager
    {
        [SerializeField] private Transform m_LeftHand;

        [SerializeField] private Transform m_RightHand;

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
            //Debug.Log($"[InputManagerAppleVisionPro] GetButtonDown: {button}");
            return false;
        }

        public override bool GetGrabDown(HandType handType)
        {
            //Debug.Log($"[InputManagerAppleVisionPro] GetGrabDown: {handType}");
            if (m_HandGestureManager == null)
            {
                m_HandGestureManager = FindObjectOfType<HandGestureManager>();
                if (m_HandGestureManager == null)
                    return false;
            }

            Handedness handedness = handType == HandType.Left ? Handedness.Left : handType == HandType.Right ? Handedness.Right : Handedness.Invalid;
            if (m_HandGestureManager.HandGestures[handedness] == HandGesture.Pinching)
                return true;

            return false;
        }

        public override bool GetGrabUp(HandType handType)
        {
            //Debug.Log($"[InputManagerAppleVisionPro] GetGrabUp: {handType}");
            return false;
        }

        public override float GetGrabValue(HandType handType)
        {
            //Debug.Log($"[InputManagerAppleVisionPro] GetGrabValue: {handType}");
            return 0f;
        }

        public override Transform GetHand(HandType handType)
        {
            // TODO: DELETE ME
            //Debug.Log($"[InputManagerAppleVisionPro] GetHand: {handType}");
            switch (handType)
            {
                case HandType.Left:
                    return m_LeftHand;
                case HandType.Right:
                    return m_RightHand;
                default:
                    return null;
            }
        }

        public override HandType GetHandType(Transform transform)
        {
            if (transform.TryGetComponent<AppleVisionProHandCollisionController>(out var hand)) 
            {
                return hand.Handedness == Handedness.Left ? HandType.Left : HandType.Right;
            }
            else
            {
                return HandType.Unknown;
            }
        }

        public override HandType GetLastActiveHand()
        {
            // TODO: DELETE ME
            //Debug.Log($"[InputManagerAppleVisionPro] GetLastActiveHand");
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
            //Debug.Log($"[InputManagerAppleVisionPro] IsActiveHand {candidate}");
            return true;
        }

        public override bool IsDeviceIdle()
        {
            //Debug.Log($"[InputManagerAppleVisionPro] IsDeviceIdle");
            return false;
        }

        public override void SetControllerVisible(bool visible)
        {
            
        }
    }
}
