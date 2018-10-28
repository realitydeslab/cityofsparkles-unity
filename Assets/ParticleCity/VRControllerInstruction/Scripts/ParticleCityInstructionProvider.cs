using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderUtils;

[RequireComponent(typeof(InstructionController))]
public class ParticleCityInstructionProvider : MonoBehaviour
{
    private InstructionController controller;

    void Start()
    {
        controller = GetComponent<InstructionController>();
    }

    void Update()
    {
        controller.TriggerText = "";
        controller.GripText = "";
        controller.RotateText = "";
        controller.ArrowTarget = null;

        switch (TutorialStateManager.Instance.State)
        {
            case TutorialState.Idle:
                InputManager.Instance.SetControllerVisible(false);
                break;

            case TutorialState.InitialRedDot:
                controller.TriggerText = "Press Trigger to fly towards the red light. ";
                InputManager.Instance.SetControllerVisible(true);
                break;

            case TutorialState.ReachOutHand:
                controller.TriggerText = "Release Trigger and reach out your hand. ";
                InputManager.Instance.SetControllerVisible(true);
                break;

            case TutorialState.InitialRedDotMissed:
                controller.TriggerText = "You missed it. Release Trigger when getting closer. Let's try again. ";
                InputManager.Instance.SetControllerVisible(true);
                break;


            case TutorialState.TriggerByGrab:
            case TutorialState.TriggerByGrabLaterHint:
                controller.GripText = "Grab the red light to open it. ";
                InputManager.Instance.SetControllerVisible(true);
                break;

            case TutorialState.FlyWithRotate:
            {
                float angle = 0;
                if (TutorialStateManager.Instance.Source != null)
                {
                    Vector3 expectedDirection = TutorialStateManager.Instance.Source.transform.position - transform.position;
                    angle = Vector3.Angle(expectedDirection, transform.forward);
                    controller.ArrowTarget = TutorialStateManager.Instance.Source.transform;
                }

                if (Mathf.Abs(angle) > TutorialStateManager.Instance.MaxAngleToSource)
                {
                    controller.RotateText = "Rotate your hand to control flying direction. ";
                }
                else
                {
                    controller.TriggerText = "Now fly to the next red light. ";
                }
                InputManager.Instance.SetControllerVisible(true);
                break;
            }

            default:
                throw new NotImplementedException();
                break;
        }
    }
}
