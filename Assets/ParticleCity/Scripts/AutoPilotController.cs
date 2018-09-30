﻿using System.Collections;
using System.Collections.Generic;
using ParticleCities;
using UnityEngine;

[RequireComponent(typeof(ParticleCityPlayerController))]
public class AutoPilotController : MonoBehaviour
{
    private static AutoPilotController instance;
    public static AutoPilotController Instance
    {
        get
        {
            if (instance == null)
{
                instance = FindObjectOfType<AutoPilotController>();
            }

            return instance;
        }
    }

    [Header("Flying")] 
    public float MaxSpeed = 50;
    public float SlowDownRadius = 100;
    public float Accelleration = 10;
    public float RotationLerpRatio = 0.1f;
    public float RotationSmoothTime = 5.0f;
    public float MaxAngularVelocity = 30.0f;
    public float WaitTime = 10;
    public float IdleTimeOut = 10;
    public float StoryNodeTriggerIdleTimeOut = 30;

    [Header("Auto Targeting")]
    public Transform StoryRoot;
    public float TargetDistance = 10;
    public float TargetLostTimeout = 10;

    [Header("Debug")]
    public StoryNode Target = null;
    public float Speed = 0;
    public float StoppedTime;
    public float NullTargetAccumulatedTime = 0;

    private float angularVelocity;
    private ParticleCityPlayerController playerController;
    private float lastTriggeredTime;

    public bool IsAutoPilotTargetValid
    {
        get { return Target != null && Target.enabled; }
    }

    public void OnStoryNodeTriggered(StoryNode node)
    {
        lastTriggeredTime = Time.time;
    }

	void Start () {
	}
	
	void Update () {
	    if (playerController == null)
	    {
	        playerController = GetComponent<ParticleCityPlayerController>();
	    }

	    if (!IsAutoPilotTargetValid)
	    {
            findAutoPilotTarget();
	    }

        // Stop auto-pilot when there is any action
	    if ( !Target || (Time.time - playerController.LastActionTime < IdleTimeOut) && (Time.time - lastTriggeredTime < StoryNodeTriggerIdleTimeOut) )
	    {
	        Speed = 0;
	        angularVelocity = 0;
	        return;
	    }

	    Vector3 offset = Target.transform.position - InputManager.Instance.PlayerTransform.position;
	    Vector3 dir = offset.normalized;

	    offset -= dir * TargetDistance;

	    Vector3 groundDir = new Vector3(dir.x, 0, dir.z);
        Quaternion targetRotation = Quaternion.LookRotation(groundDir, Vector3.up);

	    float targetEulerY = targetRotation.eulerAngles.y;
	    float currentEulerY = InputManager.Instance.PlayerTransform.rotation.eulerAngles.y;
	    float eulerY = Mathf.SmoothDampAngle(currentEulerY, targetEulerY, ref angularVelocity, RotationSmoothTime, MaxAngularVelocity, Time.deltaTime);

        InputManager.Instance.PlayerTransform.rotation = Quaternion.Euler(0, eulerY, 0);

	    // if (Mathf.Abs(targetEulerY - currentEulerY) > 30)
	    // {
        //     // Do not move until the camera rotates to the target angle (bad idea)
	    //     return;
	    // }

	    if (offset.sqrMagnitude > SlowDownRadius * SlowDownRadius)
	    {
	        Speed = Mathf.Min(MaxSpeed, Speed + Accelleration * Time.deltaTime);
	    }
	    else
	    {
	        float deceleration = Speed * Speed / offset.magnitude / 2;
	        Speed = Mathf.Max(0, Speed - deceleration * Time.deltaTime);
	    }
	    InputManager.Instance.PlayerTransform.position += dir * Speed * Time.deltaTime;

	    if (Speed < 0.1f)
	    {
	        StoppedTime += Time.deltaTime;
	    }
	    else
	    {
	        StoppedTime = 0;
	    }

	    if (StoppedTime > WaitTime && Target != null)
	    {
            Target.GotoNext();
	    }
	}

    private void findAutoPilotTarget()
    {
        List<StoryNode> availableNodes = new List<StoryNode>(StoryRoot.GetComponentsInChildren<StoryNode>(false));
        for (int i = 0; i < availableNodes.Count; i++)
        {
            if (availableNodes[i].AutoPilotSkip)
            {
                availableNodes.RemoveAt(i);
                i--;
            }
        }

        if (availableNodes.Count == 0)
        {
            Target = null;
        }
        else if (availableNodes.Count == 1)
        {
            Target = availableNodes[0];
        }
        else
        {
            int rand = (int) (Random.value * availableNodes.Count);
            Target = availableNodes[rand];
        }

        if (Target == null)
        {
            NullTargetAccumulatedTime += Time.deltaTime;
            if (NullTargetAccumulatedTime > TargetLostTimeout)
            {
                availableNodes = new List<StoryNode>(StoryRoot.GetComponentsInChildren<StoryNode>(true));
                for (int i = 0; i < availableNodes.Count; i++)
                {
                    if (!availableNodes[i].AutoPilotSkip)
                    {
                        availableNodes[i].gameObject.SetActive(true);
                        Target = availableNodes[i];
                        break;
                    }
                }
            }
        }

        if (Target != null)
        {
            NullTargetAccumulatedTime = 0;
        }
    }
}