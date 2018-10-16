using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotation : MonoBehaviour
{
    public float Speed;

    void Start()
    {
        
    }

    void Update()
    {
        transform.Rotate(Vector3.up, Speed * Time.deltaTime);        
    }
}
