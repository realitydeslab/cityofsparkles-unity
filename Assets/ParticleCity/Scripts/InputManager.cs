using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParticleCities
{
    public enum HandType
    {
        Unknown = 0,
        Left, 
        Right
    }

    public abstract class InputManager : MonoBehaviour
    {
        public abstract Transform GetHand(HandType handType);
        public abstract Camera CenterCamera { get; }
        public abstract Transform PlayerTransform { get; }

        public abstract float GetTriggerValue(HandType handType);
        public abstract float GetGrabValue(HandType handType);

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
