using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderUtils;

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
        
        public float LastActionTime { get; private set; }

        private CharacterController characterController;

        private ParticleSystem leftParticle;
        private ParticleSystem rightParticle;
        private float particleFullRate;
        private AutoPilotController autoPilot;

        void Start()
        {
            autoPilot = GetComponent<AutoPilotController>();

            LastActionTime = Time.time;

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
            else if (rightTrigger >= leftTrigger && rightTrigger > 0.01f)
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
                Vector3 forward = activeHand.forward;

                // Tutorial Control
                bool flyAllowed = true;
                if ((TutorialStateManager.Instance.State == TutorialState.InitialRedDot ||
                    TutorialStateManager.Instance.State == TutorialState.InitialRedDotMissed ||
                     TutorialStateManager.Instance.State == TutorialState.ReachOutHand || 
                     TutorialStateManager.Instance.State == TutorialState.TriggerByGrab
                     ) &&
                    autoPilot != null && autoPilot.IsAutoPilotTargetValid)
                {
                    Vector3 direction = (autoPilot.Target.transform.position - InputManager.Instance.CenterCamera.transform.position).normalized;
                    if (Vector3.Dot(direction, InputManager.Instance.CenterCamera.transform.forward) >= 0)
                    {
                        forward = direction;
                    }
                    else
                    {
                        TutorialStateManager.Instance.InitialRedDotMissed();
                        forward = -direction;
                        StartCoroutine(resetPositionWithDelay(2));
                    }
                }
                else if (TutorialStateManager.Instance.State == TutorialState.FlyWithRotate && TutorialStateManager.Instance.Source != null)
                {
                    Vector3 expectedDirection = TutorialStateManager.Instance.Source.transform.position - activeHand.position;
                    float angle = Vector3.Angle(expectedDirection, activeHand.forward);
                    if (Mathf.Abs(angle) > TutorialStateManager.Instance.MaxAngleToSource)
                    {
                        // Stop flying
                        flyAllowed = false;
                    }
                }

                if (flyAllowed)
                {
                    Vector3 movement = forward * FlyFullSpeed * activeTrigger * Time.deltaTime;
                    InputManager.Instance.PlayerTransform.transform.position += movement;
                    LastActionTime = Time.time;
                }
            }

            ParticleSystem.EmissionModule leftEmission = leftParticle.emission;
            leftEmission.rateOverTimeMultiplier = particleFullRate * leftTrigger;

            ParticleSystem.EmissionModule rightEmission = rightParticle.emission;
            rightEmission.rateOverTimeMultiplier = particleFullRate * rightTrigger;

        }

        private IEnumerator resetPositionWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (PlayerStartingPoint != null)
            {
                InputManager.Instance.PlayerTransform.transform.position = PlayerStartingPoint.position;
                InputManager.Instance.PlayerTransform.transform.rotation = PlayerStartingPoint.rotation;
                InputManager.Instance.PlayerTransform.transform.localScale = PlayerStartingPoint.localScale;
            }
        }
    }
}
