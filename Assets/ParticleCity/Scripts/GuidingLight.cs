using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuidingLight : MonoBehaviour
{
    [Range(0.0f, 5.0f)]
    public float MaxIntensity;
    public bool Trigger;
    public float EaseInDuration;
    public float SustainDuration;
    public float EaseOutDuration;

    public float Interval;

    public Transform RenderPart;

    private Renderer lightRenderer;
    private Coroutine lightUpCouroutine;
    private AkAmbient akAmbient;

    private float timeSinceLastTrigger;
    private bool destroyRequested;
    private bool lightUpForSpawning;

    [Header("Debug")]
    [Range(0, 1)]
    public float Intensity;

	void Start ()
	{
	    akAmbient = GetComponent<AkAmbient>();
	    lightRenderer = GetComponentInChildren<Renderer>();
	    RenderPart = lightRenderer.transform;
	}
	
	void Update () {
	    if (timeSinceLastTrigger > Interval)
	    {
	        Trigger = true;
	    }

	    if (Trigger)
	    {
	        timeSinceLastTrigger = 0;
	        Trigger = false;

	        if (!destroyRequested && !lightUpForSpawning)
	        {
	            if (lightUpCouroutine != null)
	            {
	                StopCoroutine(lightUpCouroutine);
	            }

                lightUpCouroutine = StartCoroutine(lightUp(false));

                /*
                if (akAmbient != null)
                {
                    AkSoundEngine.PostEvent((uint)akAmbient.eventID, akAmbient.gameObject);
                }
                */
            }
        }

	    if (destroyRequested && lightUpCouroutine == null)
	    {
	        lightUpCouroutine = StartCoroutine(lightUp(true));
	    }

	    timeSinceLastTrigger += Time.deltaTime;
	}

    IEnumerator lightUp(bool fadeOut)
    {
        float t = fadeOut ? EaseInDuration + SustainDuration : 0;

        while (t <= EaseInDuration + SustainDuration + EaseOutDuration)
        {
            float intensity = MaxIntensity;
            if (t < EaseInDuration)
            {
                intensity = Mathf.Pow(t / EaseInDuration, 2) * MaxIntensity;
            }
            else if (t >= EaseInDuration + SustainDuration)
            {
                intensity = Mathf.Pow(1 - (t - EaseInDuration - SustainDuration) / EaseOutDuration, 2) * MaxIntensity;
            }

            setIntensity(intensity);

            t += Time.deltaTime;
            yield return null;
        }

        setIntensity(0);
        lightUpCouroutine = null;

        if (destroyRequested)
        {
            Destroy(gameObject);
            destroyRequested = false;
        }
    }

    public void LightUpForSpawning()
    {
        if (lightUpCouroutine != null)
        {
            StopCoroutine(lightUpCouroutine);
            lightUpCouroutine = null;
        }

        // TODO: State
        setIntensity(1.2f);
        lightUpForSpawning = true;
    }

    public void MarkForDestroy()
    {
        destroyRequested = true;
        lightUpForSpawning = false;
    }

    private void setIntensity(float intensity)
    {
        Intensity = intensity;
        lightRenderer.material.SetFloat("_Intensity", intensity);
    }
}
