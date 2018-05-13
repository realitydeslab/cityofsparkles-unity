using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    public Color SolidClearColor;

    void OnEnable()
    {
        Camera.main.clearFlags = CameraClearFlags.Color;
        Camera.main.backgroundColor = SolidClearColor;
    }
}
