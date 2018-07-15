using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace ParticleCities
{
    public class InputManagerSteamVR : InputManager
    {
        private SteamVR_ControllerManager controllerManager;

        void Start()
        {
            controllerManager = FindObjectOfType<SteamVR_ControllerManager>();
        }
        
        public override Transform GetHand(HandType handType)
        {
            switch (handType)
            {
                case HandType.Left:
                    return controllerManager.left.transform;

                case HandType.Right:
                    return controllerManager.right.transform;

                default:
                    return null;
            }
        }

        public override float GetTriggerValue(HandType handType)
        {
            SteamVR_Controller.Device device = getDevice(handType);
            if (device == null)
            {
                return 0;
            }

            float result = device.GetAxis(EVRButtonId.k_EButton_SteamVR_Trigger).x;

            return result;
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
                return controllerManager.transform;
            }
        }

        public override bool IsGrabContinuous
        {
            get { return false; }
        }

        public override bool HasTouchpad
        {
            get { return true; }
        }

        public override bool HasSticker
        {
            get { return false; }
        }

        public override float GetGrabValue(HandType handType)
        {
            SteamVR_Controller.Device device = getDevice(handType);
            if (device == null)
            {
                return 0;
            }

            bool press = device.GetPress(EVRButtonId.k_EButton_Grip);
            return press ? 1 : 0;
        }

        public override Vector2 GetTouchpadValue(HandType handType, out bool isPressed)
        {
            isPressed = false;

            SteamVR_Controller.Device device = getDevice(handType);
            if (device == null)
            {
                return Vector2.zero;
            }

            isPressed = device.GetPress(EVRButtonId.k_EButton_SteamVR_Touchpad);

            return device.GetAxis(EVRButtonId.k_EButton_SteamVR_Touchpad);
        }

        public override bool GetGrabDown(HandType handType)
        {
            SteamVR_Controller.Device device = getDevice(handType);
            if (device == null)
            {
                return false;
            }

            return device.GetPressDown(EVRButtonId.k_EButton_Grip);
        }

        public override bool GetGrabUp(HandType handType)
        {
            SteamVR_Controller.Device device = getDevice(handType);
            if (device == null)
            {
                return false;
            }

            return device.GetPressUp(EVRButtonId.k_EButton_Grip);
        }

        public override HandType GetHandType(Transform transform)
        {
            SteamVR_TrackedObject trackedObject = transform.GetComponentInParent<SteamVR_TrackedObject>();
            if (trackedObject == null)
            {
                return HandType.Unknown;
            }

            if (controllerManager.left == trackedObject.gameObject)
            {
                return HandType.Left;
            }
            else if (controllerManager.right == trackedObject.gameObject)
            {
                return HandType.Right;
            }
            else
            {
                return HandType.Unknown;
            }
        }

        private SteamVR_Controller.Device getDevice(HandType handType)
        {
            GameObject deviceGameObject = null;
            switch (handType)
            {
                case HandType.Left:
                    deviceGameObject = controllerManager.left;
                    break;

                case HandType.Right:
                    deviceGameObject = controllerManager.right;
                    break;
            }

            if (deviceGameObject == null)
            {
                return null;
            }

            SteamVR_TrackedObject.EIndex deviceIndex = deviceGameObject.GetComponent<SteamVR_TrackedObject>().index;
            if (deviceIndex >= 0)
            {
                return SteamVR_Controller.Input((int) deviceIndex);
            }
            else
            {
                return null;
            }
        }
    }
}
