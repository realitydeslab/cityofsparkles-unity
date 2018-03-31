using System;
using UnityEngine;
using System.Collections;
using ParticleCities;
using UnityEngine.Rendering;

public class ShakeEscapeMotion : ParticleMotionBase
{
    public Material ParticleCityMaterial;

    public bool Impulse;
    public float ImpulseScale = 200;

    public AnimationCurve VolumeToImpulse;

    private InteractiveMusicController interactiveMusic;
    private ParticleCityAnimator animator;

    public override void Start()
    {
        base.Start();

        interactiveMusic = GetComponentInChildren<InteractiveMusicController>();
        animator = GetComponent<ParticleCityAnimator>();

        if (interactiveMusic != null)
        {
            interactiveMusic.AkMarkerTriggered += InteractiveMusicOnAkMarkerTriggered;
        }
    }

    public override void OnDestroy()
    {
        if (interactiveMusic != null)
        {
            interactiveMusic.AkMarkerTriggered -= InteractiveMusicOnAkMarkerTriggered;
        }

        base.OnDestroy();
    }

    private void InteractiveMusicOnAkMarkerTriggered(string marker)
    {
        Debug.Log("Event triggered");
        if (marker == "Impulse")
        {
            ImpulseScale = 200;
            Impulse = true;
        }
        else if (marker == "Kick")
        {
            ImpulseScale = 50;
            Impulse = true;
        }
    }

    protected override void UpdateInput()
    {
        Transform leftHand = InputManager.Instance.GetHand(HandType.Left);
        Transform rightHand = InputManager.Instance.GetHand(HandType.Right);
        Transform centerEye = InputManager.Instance.CenterCamera.transform;

        Vector3 leftForward = leftHand.forward;
        if (InputManager.Instance.GetTriggerValue(HandType.Left) > 0.5f)
        {
            leftForward *= -1;
        }
        Vector3 rightForward = -rightHand.forward;
        if (InputManager.Instance.GetTriggerValue(HandType.Right) > 0.5f)
        {
            rightForward *= -1;
        }
        
        // Update input
        particleMotionBlitMaterial.SetVector("_RightHandPos", new Vector4(rightHand.position.x, rightHand.position.y, rightHand.position.z, 1));
        particleMotionBlitMaterial.SetVector("_LeftHandPos", new Vector4(leftHand.position.x, leftHand.position.y, leftHand.position.z, 1));
        particleMotionBlitMaterial.SetVector("_RightHandForward", new Vector4(rightForward.x, rightForward.y, rightForward.z, 1));
        particleMotionBlitMaterial.SetVector("_LeftHandForward", new Vector4(leftForward.x, leftForward.y, leftForward.z, 1));
        particleMotionBlitMaterial.SetVector("_HeadPos", new Vector4(centerEye.position.x, centerEye.position.y, centerEye.position.z, 1));

        if (Impulse)
        {
            Impulse = false;
            particleMotionBlitMaterial.SetFloat("_ImpulseScale", ImpulseScale);
        }
        else
        {
            particleMotionBlitMaterial.SetFloat("_ImpulseScale", 0);
        }

        float meter = interactiveMusic.GetVolumeMeter();
        float deltaHeight = VolumeToImpulse.Evaluate(meter);
        // particleMotionBlitMaterial.SetFloat("_VerticalImpulse", verticalImpulse);
        animator.SetMaterialsFloat("_VolumeDeltaHeight", deltaHeight);

        // Hand Gravity
        float limit = 0.8f;
        limit *= (Mathf.Clamp(meter, -6, 0) + 6) / 6 * 0.2f + 0.9f;

        // TODO: Broken for Vive
        float leftTrigger = InputManager.Instance.GetGrabValue(HandType.Left);
        if (leftTrigger < 0.01f)
        {
            leftTrigger = 0;
        }

        float rightTrigger = InputManager.Instance.GetGrabValue(HandType.Right);
        if (rightTrigger < 0.01f)
        {
            rightTrigger = 0;
        }
        particleMotionBlitMaterial.SetFloat("_LeftHandGravity", leftTrigger * limit);
        particleMotionBlitMaterial.SetFloat("_RightHandGravity", rightTrigger * limit);
    }
}
