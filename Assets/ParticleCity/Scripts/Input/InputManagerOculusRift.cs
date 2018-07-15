﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParticleCities
{
    public class InputManagerOculusRift : InputManager
    {
        private OvrAvatar avatar;
        private OVRCameraRig cameraRig;

        void Start()
        {
            avatar = FindObjectOfType<OvrAvatar>();
            cameraRig = FindObjectOfType<OVRCameraRig>();
        }
        
        public override Transform GetHand(HandType handType)
        {
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
            get { return cameraRig.transform; }
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
    }
}
