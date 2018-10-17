using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderUtils;

namespace ParticleCities
{
    public class InputManagerOculusRift : InputManager
    {
        private OvrAvatar avatar;
        private OVRCameraRig cameraRig;

        public override Transform GetHand(HandType handType)
        {
            if (avatar == null)
            {
                avatar = FindObjectOfType<OvrAvatar>();
            }

            switch (handType)
            {
                case HandType.Left:
                    return avatar.HandLeft.transform;

                case HandType.Right:
                    return avatar.HandRight.transform;

                default:
                    return null;
            }
        }

        public override float GetTriggerValue(HandType handType)
        {
            return OVRInput.Get(handType == HandType.Left ? OVRInput.RawAxis1D.LIndexTrigger : OVRInput.RawAxis1D.RIndexTrigger);
        }

        public override Camera CenterCamera
        {
            get { return Camera.main; }
        }

        public override Transform PlayerTransform
        {
            get
            {
                if (cameraRig == null)
                {
                    cameraRig = FindObjectOfType<OVRCameraRig>();
                }
                return cameraRig.transform;
            }
        }

        public override bool IsGrabContinuous
        {
            get { return true; }
        }

        public override bool HasTouchpad
        {
            get { return false; }
        }

        public override bool HasSticker
        {
            get { return true; }
        }

        public override float GetGrabValue(HandType handType)
        {
            return OVRInput.Get(handType == HandType.Left ? OVRInput.RawAxis1D.LHandTrigger : OVRInput.RawAxis1D.RHandTrigger);
        }

        public override Vector2 GetTouchpadValue(HandType handType, out bool isPressed)
        {
            isPressed = false;
            return Vector2.zero;
        }

        public override bool GetGrabUp(HandType handType)
        {
            return OVRInput.GetUp(handType == HandType.Left ? OVRInput.RawButton.LHandTrigger : OVRInput.RawButton.RHandTrigger);
        }

        public override bool GetGrabDown(HandType handType)
        {
            return OVRInput.GetDown(handType == HandType.Left ? OVRInput.RawButton.LHandTrigger : OVRInput.RawButton.RHandTrigger);
        }

        public override HandType GetHandType(Transform transform)
        {
            OvrAvatarHand hand = transform.GetComponentInParent<OvrAvatarHand>();
            if (hand == null)
            {
                return HandType.Unknown;
            }

            if (hand.gameObject.name == "hand_left")
            {
                return HandType.Left;
            }
            else if (hand.gameObject.name == "hand_right")
            {
                return HandType.Right;
            }
            else
            {
                return HandType.Unknown;
            }
        }

        public override bool GetButtonDown(Button button)
        {
            // TODO
            OVRInput.RawButton? rawButton;
            switch (button)
            {
                case Button.A:
                case Button.Confirm:
                    rawButton = OVRInput.RawButton.A;
                    break;

                case Button.B:
                    rawButton = OVRInput.RawButton.B;
                    break;

                default:
                    rawButton = null;
                    break;
            }

            if (rawButton.HasValue)
            {
                return OVRInput.GetDown(rawButton.Value);
            }
            else
            {
                return false;
            }
        }
    }
}
