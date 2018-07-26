using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParticleCities
{
    public class ParticleCityPlayerController : MonoBehaviour
    {
        public Transform PlayerStartingPoint;

        public bool FlyMode
        {
            get { return true; }
        }

        public float FlyFullSpeed = 100;
        public bool TrailingParticle = true;

        // private OVRPlayerController ovrPlayerController;
        private CharacterController characterController;

        private ParticleSystem leftParticle;
        private ParticleSystem rightParticle;
        private float particleFullRate;

        void Start()
        {
            // TODO: Cross platform
            // ovrPlayerController = playerTransform.GetComponent<OVRPlayerController>();
            // characterController = playerTransform.GetComponent<CharacterController>();

            Transform playerTransform = InputManager.Instance.PlayerTransform;

            leftParticle = InputManager.Instance.GetHand(HandType.Left).GetComponentInChildren<ParticleSystem>(true);
            rightParticle = InputManager.Instance.GetHand(HandType.Right).GetComponentInChildren<ParticleSystem>(true);
            particleFullRate = leftParticle.emission.rateOverTimeMultiplier;

            if (PlayerStartingPoint != null)
            {
                playerTransform.position = PlayerStartingPoint.position;
                playerTransform.rotation = PlayerStartingPoint.rotation;
                playerTransform.localScale = PlayerStartingPoint.localScale;
            }

        }

        void Update()
        {
            // ovrPlayerController.enabled = !FlyMode;
            // characterController.enabled = !FlyMode;

            leftParticle.gameObject.SetActive(FlyMode && TrailingParticle);
            rightParticle.gameObject.SetActive(FlyMode && TrailingParticle);

            if (FlyMode && TrailingParticle)
            {
                Material handParticleMat = ParticleCity.Current.HandParticleMaterial;
                if (handParticleMat != null)
                {
                    leftParticle.GetComponent<ParticleSystemRenderer>().material = handParticleMat;
                    rightParticle.GetComponent<ParticleSystemRenderer>().material = handParticleMat;
                }
            }


            if (!Mathf.Approximately(InputManager.Instance.PlayerTransform.localScale.x, ParticleCity.Current.PlayerScale))
            {
                Vector3 prevCenter = InputManager.Instance.CenterCamera.transform.position;
                InputManager.Instance.PlayerTransform.localScale = ParticleCity.Current.PlayerScale * Vector3.one;
                Vector3 centerOffset = InputManager.Instance.CenterCamera.transform.position - prevCenter;
                InputManager.Instance.PlayerTransform.position -= centerOffset;
            }

            if (FlyMode)
            {
                updateFlyMode();
            }
        }

        private void updateFlyMode()
        {
            float leftTrigger = InputManager.Instance.GetTriggerValue(HandType.Left);
            float rightTrigger = InputManager.Instance.GetTriggerValue(HandType.Right);

            Transform activeHand = null;
            float activeTrigger = 0;

            if (leftTrigger > rightTrigger && leftTrigger > 0.01f)
            {
                activeHand = InputManager.Instance.GetHand(HandType.Left);
                activeTrigger = leftTrigger;
                rightTrigger = 0;
            }
            else if (rightTrigger > leftTrigger && rightTrigger > 0.01f)
            {
                activeHand = InputManager.Instance.GetHand(HandType.Right);
                activeTrigger = rightTrigger;
                leftTrigger = 0;
            }
            else
            {
                leftTrigger = 0;
                rightTrigger = 0;
            }

            if (activeHand != null)
            {
                Vector3 movement = activeHand.forward * FlyFullSpeed * activeTrigger * Time.deltaTime;
                InputManager.Instance.PlayerTransform.transform.position += movement;
            }

            ParticleSystem.EmissionModule leftEmission = leftParticle.emission;
            leftEmission.rateOverTimeMultiplier = particleFullRate * leftTrigger;

            ParticleSystem.EmissionModule rightEmission = rightParticle.emission;
            rightEmission.rateOverTimeMultiplier = particleFullRate * rightTrigger;

        }
    }
}
