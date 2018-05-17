using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryNode : MonoBehaviour
{
    public bool EnableOnAwake;

    void Awake()
    {
        if (!EnableOnAwake)
        {
            enabled = false;
        }
    }

	public virtual void Start () {
		
	}

    public virtual void OnEnable()
    {

    }
	
	public virtual void Update () 
	{
		
	}
}
