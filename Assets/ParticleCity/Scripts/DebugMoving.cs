using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMoving : MonoBehaviour
{

    public Vector3 Velocity;

	void Update ()
	{
	    transform.position += Velocity * Time.deltaTime;
	}
}
