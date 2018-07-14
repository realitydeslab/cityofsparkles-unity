using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFPS : MonoBehaviour
{
    public int TargetFrameRate = 60;

	void Update ()
	{
	    Application.targetFrameRate = TargetFrameRate;
	}
	
}
