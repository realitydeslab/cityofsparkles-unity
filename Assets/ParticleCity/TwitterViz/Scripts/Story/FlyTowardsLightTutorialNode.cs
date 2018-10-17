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

    [Header("Debug")] 
    public float alpha;
    public float distanceToTarget;

    private Image image;
    private TMP_Text text;
    private SpriteRenderer redDot;

    public override void Start()
    {
        base.Start();

        image = GetComponentInChildren<Image>();
        text = GetComponentInChildren<TMP_Text>();
        redDot = GetComponentInChildren<SpriteRenderer>();
    }

    public override void Update()
    {
        if (Target == null || Target.IsTriggered)
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
