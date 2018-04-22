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

        void Start()
        {
            akAmbient = GetComponent<AkAmbient>();
        }

        void Update()
        {
            transform.position = InputManager.Instance.PlayerTransform.position;
            Transform leftHand = InputManager.Instance.GetHand(HandType.Left);
        }

        private Vector3 getRelativePosition(Vector3 worldPosition)
        {
            Bounds b = boundsCollider.bounds;
            Vector3 offset = (worldPosition - b.min);
            Vector3 total = (b.max - b.min);
            return new Vector3(
                offset.x / total.x,
                offset.y / total.y,
                offset.z / total.z
            );
        }
    }
}
