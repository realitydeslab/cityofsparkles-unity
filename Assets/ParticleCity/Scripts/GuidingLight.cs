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

    private Renderer lightRenderer;
    private Coroutine lightUpCouroutine;
    private AkAmbient akAmbient;

    private float timeSinceLastTrigger;
    private bool destroyRequested;

	void Start ()
	{
	    akAmbient = GetComponent<AkAmbient>();
	    lightRenderer = GetComponentInChildren<Renderer>();
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

	        if (!destroyRequested)
	        {
	            if (lightUpCouroutine != null)
	            {
	                StopCoroutine(lightUpCouroutine);
	            }

                lightUpCouroutine = StartCoroutine(lightUp());

                /*
                if (akAmbient != null)
                {
                    AkSoundEngine.PostEvent((uint)akAmbient.eventID, akAmbient.gameObject);
                }
                */
            }
        }

	    timeSinceLastTrigger += Time.deltaTime;

	    if (lightUpCouroutine == null && destroyRequested)
	    {
	        Destroy(gameObject);
	    }
	}

    IEnumerator lightUp()
    {
        float t = 0;

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

        if (destroyRequested)
        {
            Destroy(gameObject);
        }
    }

    public void MarkForDestroy()
    {
        destroyRequested = true;
        if (lightUpCouroutine == null)
        {
            Destroy(gameObject);
        }
    }

    private void setIntensity(float intensity)
    {
        lightRenderer.material.SetFloat("_Intensity", intensity);
    }
}
