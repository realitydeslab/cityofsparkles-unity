using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderUtils;

namespace ParticleCities
{
    public class InputManagerAnimated : InputManager
    {
        public Transform LeftHand;
        public Transform RightHand;

        [Range(0, 1)]
        public float LeftTrigger;

        [Range(0, 1)]
        public float RightTrigger;

        [Range(0, 2)]
        public float LeftGrab;

        [Range(0, 2)]
        public float RightGrab;

        public Vector2 LeftTouchPad;
        public bool LeftTouchPadClicked;

        public Vector2 RightTouchPad;
        public bool RightTouchPadClicked;

        public override Transform GetHand(HandType handType)
        {
            switch (handType)
            {
                case HandType.Left:
                    return LeftHand;

                case HandType.Right:
                    return RightHand;

                default:
                    return null;
            }
        }

        public override float GetTriggerValue(HandType handType)
        {
            return handType == HandType.Left ? LeftTrigger : RightTrigger;
        }

        public override Camera CenterCamera
        {
            get
            {
                // TODO
                return Camera.main;
            }
        }

        public override Transform PlayerTransform
        {
            get
            {
                return transform;
            }
        }

        public override bool IsGrabContinuous
        {
            get { return true; }
        }

        public override bool HasTouchpad
        {
            get { return true; }
        }

        public override bool HasSticker
        {
            get { return true; }
        }

        public override float GetGrabValue(HandType handType)
        {
            return handType == HandType.Left ? LeftGrab : RightGrab;
        }

        public override Vector2 GetTouchpadValue(HandType handType, out bool isPressed)
        {
            isPressed = (handType == HandType.Left ? LeftTouchPadClicked : RightTouchPadClicked);
            return (handType == HandType.Left ? LeftTouchPad : RightTouchPad);
        }

        public override bool GetGrabDown(HandType handType)
        {
            // TODO
            return false;
        }

        public override bool GetGrabUp(HandType handType)
        {
            // TODO
            return false;
        }

        public override HandType GetHandType(Transform transform)
        {
            if (transform == LeftHand || transform.IsChildOf(LeftHand))
            {
                return HandType.Left;
            }
            else if (transform == RightHand || transform.IsChildOf(RightHand))
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
            return false;
        }

        public override bool IsDeviceIdle()
        {
            return false;
        }

        private void OnDrawGizmos()
        {
            if (RightHand != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(RightHand.position, 20);
                Gizmos.DrawRay(RightHand.position, RightHand.forward * 60);
            }

            if (LeftHand != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(LeftHand.position, 20);
                Gizmos.DrawRay(LeftHand.position, LeftHand.forward * 60);
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 20);
        }
    }
}