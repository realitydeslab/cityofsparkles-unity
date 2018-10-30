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
        public Vector3 CurrentVelocity { get; private set; }

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
            Vector2 leftSticker = InputManager.Instance.GetStickerValue(HandType.Left);
            Vector2 rightSticker = InputManager.Instance.GetStickerValue(HandType.Right);

            Transform activeHand = null;
            HandType activeHandType = HandType.Unknown;
            Vector3 velocityRatio = Vector3.forward;
            CurrentVelocity = Vector3.zero;

            if (leftTrigger > rightTrigger && leftTrigger > 0.01f)
            {
                activeHandType = HandType.Left;
                velocityRatio *= leftTrigger;
            }
            else if (rightTrigger >= leftTrigger && rightTrigger > 0.01f)
            {
                activeHandType = HandType.Right;
                velocityRatio *= rightTrigger;
            }
            else if (leftSticker.sqrMagnitude > rightSticker.sqrMagnitude && leftSticker.sqrMagnitude > 0.0001f)
            {
                activeHandType = HandType.Left;
                velocityRatio = new Vector3(leftSticker.x * 0.3f, 0, leftSticker.y);
            }
            else if (rightSticker.sqrMagnitude >= leftSticker.sqrMagnitude && rightSticker.sqrMagnitude > 0.0001f)
            {
                activeHandType = HandType.Right;
                velocityRatio = new Vector3(rightSticker.x * 0.3f, 0, rightSticker.y);
            }

            activeHand = InputManager.Instance.GetHand(activeHandType);
            if (activeHand != null)
            {
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
                        velocityRatio = velocityRatio.magnitude * direction;
                    }
                    else
                    {
                        TutorialStateManager.Instance.InitialRedDotMissed();
                        velocityRatio = velocityRatio.magnitude * -direction;
                        StartCoroutine(resetPositionWithDelay(TutorialState.InitialRedDotMissed, 2));
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
                        velocityRatio = Vector3.zero;
                    }
                    else
                    {
                        velocityRatio = velocityRatio.magnitude * activeHand.forward;
                    }
                }
                else
                {
                    velocityRatio = activeHand.rotation * velocityRatio;
                }

                if (flyAllowed)
                {
                    CurrentVelocity = velocityRatio * FlyFullSpeed;
                    InputManager.Instance.PlayerTransform.transform.position += CurrentVelocity * Time.deltaTime;
                    LastActionTime = Time.time;
                }
            }

            ParticleSystem.EmissionModule leftEmission = leftParticle.emission;
            leftEmission.rateOverTimeMultiplier = particleFullRate * (activeHandType == HandType.Left ? velocityRatio.magnitude : 0);

            ParticleSystem.EmissionModule rightEmission = rightParticle.emission;
            rightEmission.rateOverTimeMultiplier = particleFullRate * (activeHandType == HandType.Right ? velocityRatio.magnitude : 0);
        }

        private IEnumerator resetPositionWithDelay(TutorialState requiredState, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (requiredState == TutorialStateManager.Instance.State && PlayerStartingPoint != null)
            {
                InputManager.Instance.PlayerTransform.transform.position = PlayerStartingPoint.position;
                InputManager.Instance.PlayerTransform.transform.rotation = PlayerStartingPoint.rotation;
                InputManager.Instance.PlayerTransform.transform.localScale = PlayerStartingPoint.localScale;
            }
        }
    }
}
