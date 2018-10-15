using System.Collections;
using System.Collections.Generic;
using ParticleCities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WanderUtils;

public class FlyTowardsLightTutorialNode : StoryNode
{
    public StoryNode Target;
    public AnimationCurve AlphaOverDistanceCurve;

    public float DistanceToCamera = 100;
    public float CameraLerpRatio = 1;

    [Header("Debug")] 
    public float alpha;
    public float distanceToTarget;
    public bool following = false;

    private Image image;
    private TMP_Text text;
    private SpriteRenderer redDot;

    void Start()
    {
        image = GetComponentInChildren<Image>();
        text = GetComponentInChildren<TMP_Text>();
        redDot = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        Vector3 targetPosition = InputManager.Instance.CenterCamera.transform.position + InputManager.Instance.CenterCamera.transform.forward * DistanceToCamera;
        float distSq = (targetPosition - transform.position).sqrMagnitude;

        if (distSq > 1600)
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
            transform.forward = transform.position - InputManager.Instance.CenterCamera.transform.position;
        }

        if (Target == null)
        {
            Destroy(gameObject);
            return;
        }

        distanceToTarget = Vector3.Distance(InputManager.Instance.CenterCamera.transform.position, Target.transform.position);
        alpha = AlphaOverDistanceCurve.Evaluate(distanceToTarget);

        image.color = image.color.ColorWithAlpha(alpha);
        text.color = text.color.ColorWithAlpha(alpha);
        redDot.color = redDot.color.ColorWithAlpha(alpha);
    }
}
