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
        switch (TutorialStateManager.Instance.State)
        {
            case TutorialState.Idle:
                controller.TriggerText = "";
                controller.GripText = "";
                InputManager.Instance.SetControllerVisible(false);
                break;

            case TutorialState.InitialRedDot:
                controller.TriggerText = "Press Trigger to fly towards the red light. ";
                controller.GripText = "";
                InputManager.Instance.SetControllerVisible(true);
                break;

            case TutorialState.ReachOutHand:
                controller.TriggerText = "Release Trigger and reach out your hand. ";
                controller.GripText = "";
                InputManager.Instance.SetControllerVisible(true);
                break;

            case TutorialState.InitialRedDotMissed:
                controller.TriggerText = "You missed it. Release Trigger when getting closer. Let's try again. ";
                controller.GripText = "";
                InputManager.Instance.SetControllerVisible(true);
                break;


            case TutorialState.TriggerByGrab:
            case TutorialState.TriggerByGrabLaterHint:
                controller.TriggerText = "";
                controller.GripText = "Grab the red light to open it. ";
                InputManager.Instance.SetControllerVisible(true);
                break;

            default:
                throw new NotImplementedException();
                break;
        }
    }
}
