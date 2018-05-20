using System;
using System.Collections;
using System.Collections.Generic;
using ParticleCities;
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

    public GameObject RisingParticlePrefab;

    public bool MusicSync;

    private Renderer lightRenderer;
    private Coroutine lightUpCouroutine;
    private AkAmbient akAmbient;

    private float timeSinceLastTrigger = float.MaxValue;
    private bool destroyRequested;
    private bool turnOffRequested;
    private bool lightUpForSpawning;
    private ParticleSystem particleSystem;

    [Header("Debug")]
    [Range(0, 1)]
    public float Intensity;

    void Start ()
	{
	    akAmbient = GetComponent<AkAmbient>();
	    lightRenderer = GetComponentInChildren<Renderer>();

        // TODO: Pooling
	    particleSystem = GetComponentInChildren<ParticleSystem>(true);

	    RenderPart = lightRenderer.transform;

	    if (StageSwitcher.Instance.CurrentStage == Stage.Twist)
	    {
	        Instantiate(RisingParticlePrefab, this.transform);
	    }

        setIntensity(0);
        if (MusicSync)
        {
            InteractiveMusicController.Instance.AkMusicSyncCueTriggered += OnAkMusicSyncCueTriggered;
            InteractiveMusicController.Instance.AddPointOfInterest(gameObject);
        }
	}
	
	void Update () {
	    if (!MusicSync && timeSinceLastTrigger > Interval)
	    {
	        Trigger = true;
	    }

	    if (Trigger)
	    {
	        timeSinceLastTrigger = 0;
	        Trigger = false;

	        if (!destroyRequested && !turnOffRequested && !lightUpForSpawning)
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

	    if ((destroyRequested || turnOffRequested) && lightUpCouroutine == null)
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
        else if (turnOffRequested)
        {
            enabled = false;
        }
    }

    void OnDestroy()
    {
        if (InteractiveMusicController.Instance != null)
        {
            InteractiveMusicController.Instance.AkMusicSyncCueTriggered -= OnAkMusicSyncCueTriggered;
            InteractiveMusicController.Instance.RemovePointOfInterest(gameObject);
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
        setIntensity(1.5f);
        lightUpForSpawning = true;
        // particleSystem.gameObject.SetActive(true);
    }

    public void TurnOff()
    {
        turnOffRequested = true;
    }

    public void MarkForDestroy()
    {
        destroyRequested = true;
        lightUpForSpawning = false;
        // particleSystem.gameObject.SetActive(false);
    }

    private void setIntensity(float intensity)
    {
        Intensity = intensity;
        lightRenderer.material.SetFloat("_Intensity", intensity);
    }

    private void OnAkMusicSyncCueTriggered(string cue)
    {
        Debug.Log("Light sync: " + cue);
        string[] comp = cue.Split(':');
        EaseInDuration = float.Parse(comp[1]);
        SustainDuration = float.Parse(comp[2]);
        EaseOutDuration = float.Parse(comp[3]);
        Trigger = true;
    }

}
