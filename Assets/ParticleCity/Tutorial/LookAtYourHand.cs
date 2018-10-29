using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WanderUtils;

public class LookAtYourHand : MonoBehaviour
{
    public float FadeRatio = 1f;
    public float WaitForInvisibleTime = 5f;

    private TextMeshProUGUI text;
    private float accumulatedWaitTime = 0;

    [Header("Debug")]
    public bool ShouldShowText;

    void Start()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        text.color = Color.clear;
        ShouldShowText = false;
    }

    void Update()
    {
        bool showText = false;
        if (TutorialStateManager.Instance.State != TutorialState.Idle &&
            TutorialStateManager.Instance.State != TutorialState.Invalid)
        {
            bool leftVisible = isVisible(InputManager.Instance.CenterCamera, InputManager.Instance.GetHand(HandType.Left));
            bool rightVisible = isVisible(InputManager.Instance.CenterCamera, InputManager.Instance.GetHand(HandType.Right));
            bool anyVisible = leftVisible || rightVisible;

            if (anyVisible)
            {
                accumulatedWaitTime = 0;
                ShouldShowText = false;
            }
            else
            {
                accumulatedWaitTime += Time.deltaTime;
                if (accumulatedWaitTime > WaitForInvisibleTime)
                {
                    ShouldShowText = true;
                }
            }
        }

        text.color = Color.Lerp(text.color, (ShouldShowText ? Color.white : Color.clear), FadeRatio * Time.deltaTime);
    }

    private bool isVisible(Camera camera, Transform rendererObject)
    {
        Renderer r = rendererObject.GetComponentInChildren<Renderer>();
        if (r == null)
        {
            return false;
        }

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, r.bounds);
    }
}
