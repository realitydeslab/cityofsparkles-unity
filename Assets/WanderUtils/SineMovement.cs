using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineMovement : MonoBehaviour
{
    public Vector3 Direction = new Vector3(1, 0, 0);
    public float Amplitude = 1;
    public float Period = 1;

    public bool ResetInitialPosition;

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        if (ResetInitialPosition)
        {
            initialPosition = transform.localPosition;
            ResetInitialPosition = false;
        }

        Vector3 movement = Direction.normalized * Amplitude * Mathf.Sin(Mathf.PI * 2 / Period * Time.time);
        transform.localPosition = initialPosition + movement;
    }
}
