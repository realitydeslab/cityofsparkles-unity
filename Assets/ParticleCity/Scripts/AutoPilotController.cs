using System.Collections;
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
    public float SlowDownRatio = 0.2f;
    public float Accelleration = 10; 

    [Header("Auto Targeting")]
    public Transform StoryRoot;

    [Header("Debug")]
    public StoryNode Target = null;
    public bool StoryFinished = false;
    public float Speed = 0;

    public bool IsAutoPilotTargetValid
    {
        get { return Target != null && Target.enabled; }
    }

	void Start () {
		
	}
	
	void Update () {
	    if (!StoryFinished && !IsAutoPilotTargetValid)
	    {
            findAutoPilotTarget();
	    }

	    Vector3 offset = Target.transform.position - InputManager.Instance.PlayerTransform.position;
	    if (offset.sqrMagnitude > SlowDownRadius * SlowDownRadius)
	    {
	        Speed = Mathf.Min(MaxSpeed, Speed + Accelleration * Time.deltaTime);
	    }
	    else
	    {
	        float deceleration = Speed * Speed / offset.magnitude / 2;
	        Speed = Mathf.Max(0, Speed - deceleration * Time.deltaTime);
	    }

	    InputManager.Instance.PlayerTransform.position += offset.normalized * Speed * Time.deltaTime;
	}

    private void findAutoPilotTarget()
    {
        StoryNode[] availableNodes = StoryRoot.GetComponentsInChildren<StoryNode>(false);
        if (availableNodes.Length == 0)
        {
            Target = null;
            StoryFinished = true;
        }
        else if (availableNodes.Length == 1)
        {
            Target = availableNodes[0];
        }
        else
        {
            int rand = (int) (Random.value * availableNodes.Length);
            Target = availableNodes[rand];
        }
    }
}
