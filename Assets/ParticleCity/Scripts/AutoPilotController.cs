using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public Transform StoryRoot;

    [Header("Debug")]
    public StoryNode Target = null;
    public bool StoryFinished = false;

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
