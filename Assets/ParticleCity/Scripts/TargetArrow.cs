using System.Collections;
using System.Collections.Generic;
using ParticleCities;
using UnityEngine;
using WanderUtils;

public class TargetArrow : MonoBehaviour
{
    public float DistanceToCamera = 100;
    public float Radius = 50;
    public Transform Target;
    public float MinAngle = 90;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    
    void Update()
    {
        float targetAlpha = 0;

        if (Target != null)
        {
            Transform camTransform = InputManager.Instance.CenterCamera.transform;
            Vector3 camToTarget = Target.position - camTransform.position;

            float angle = Vector3.Angle(camTransform.forward, camToTarget);
            targetAlpha = (Mathf.Abs(angle) > MinAngle) ? 1f : 0f;

            // Project vector camToTarget onto camera plane
            Vector3 A = camToTarget.normalized;
            Vector3 N = camTransform.forward;
            Vector3 P = Vector3.Cross(N, Vector3.Cross(A, N));

            transform.position = camTransform.position + DistanceToCamera * camTransform.forward + P * Radius;
            transform.rotation = Quaternion.LookRotation(camTransform.forward, P);
        }

        float alpha = Mathf.Lerp(spriteRenderer.color.a, targetAlpha, 10 * Time.deltaTime);
        spriteRenderer.color = spriteRenderer.color.ColorWithAlpha(alpha);
    }
}
