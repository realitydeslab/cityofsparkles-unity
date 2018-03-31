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

        public override float GetGrabValue(HandType handType)
        {
            SteamVR_Controller.Device device = getDevice(handType);
            float result = device.GetAxis(EVRButtonId.k_EButton_SteamVR_Trigger).x;

            return result;
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
