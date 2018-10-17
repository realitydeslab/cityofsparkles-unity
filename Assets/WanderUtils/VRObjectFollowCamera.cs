using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderUtils;

public class VRObjectFollowCamera : MonoBehaviour
{
    public float DistanceToCamera = 100;
    public float CameraLerpRatio = 1;
    public float MinDistance = 40;

    public Transform CameraTransform;

    private bool following = false;

    void Start()
    {
        if (CameraTransform == null)
        {
            CameraTransform = InputManager.Instance.CenterCamera.transform;
        }

        transform.position = CameraTransform.position + CameraTransform.forward * DistanceToCamera;
        transform.forward = transform.position - CameraTransform.position;
    }

    void Update()
    {
        if (CameraTransform == null)
        {
            CameraTransform = InputManager.Instance.CenterCamera.transform;
        }

        Vector3 targetPosition = CameraTransform.position + CameraTransform.forward * DistanceToCamera;
        float distSq = (targetPosition - transform.position).sqrMagnitude;

        if (distSq > 1000000)
        {
            transform.position = targetPosition;
            transform.forward = targetPosition - CameraTransform.position;
        }
        else if (distSq > MinDistance * MinDistance)
        {
            following = true;
        }
        else if (distSq < 4)
        {
            following = false;
        }

        if (following)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, CameraLerpRatio * Time.deltaTime);
            transform.forward = transform.position - CameraTransform.position;
        }

    }
}
