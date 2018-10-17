using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WanderUtils
{
    public enum HandType
    {
        Unknown = 0,
        Left, 
        Right
    }

    public enum Button
    {
        Unknown = 0,
        A,
        B,
        Confirm,
        TouchPad
    }

    public abstract class InputManager : MonoBehaviour
    {
        public abstract Transform GetHand(HandType handType);
        public abstract Camera CenterCamera { get; }
        public abstract Transform PlayerTransform { get; }

        public abstract float GetTriggerValue(HandType handType);
        public abstract float GetGrabValue(HandType handType);
        public abstract Vector2 GetTouchpadValue(HandType handType, out bool isPressed);
        public abstract bool GetGrabDown(HandType handType);
        public abstract bool GetGrabUp(HandType handType);

        public abstract bool IsGrabContinuous { get; }
        public abstract bool HasSticker { get; }
        public abstract bool HasTouchpad { get; }

        public abstract HandType GetHandType(Transform transform);

        public abstract bool GetButtonDown(Button button);

        private static InputManager instance = null;

        public static InputManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<InputManager>();
                }

                return instance;
            }
        }
    }
}
