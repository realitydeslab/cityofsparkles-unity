using System;
using System.Collections;
using System.Collections.Generic;
using ParticleCities;
using UnityEngine;

public class GuidingLight : MonoBehaviour
{
    private enum FadeType
    {
        FadeInOut = 0,
        FadeOut
    }

    [Range(0.0f, 5.0f)]
    public float MaxIntensity;
    [Range(0.0f, 1.0f)]
    public float MinIntensity;

    public bool Trigger;
    public float EaseInDuration;
    public float SustainDuration;
    public float EaseOutDuration;

    public float Interval;

    public Transform RenderPart;

    public GameObject RisingParticlePrefab;

    public bool MusicSync;

    private Renderer lightRenderer;
    private Coroutine smoothLightCoroutine;
    private FadeType currentFadeType;
    private AkAmbient akAmbient;

    private float timeSinceLastTrigger = float.MaxValue;
    // private ParticleSystem particleSystem;

    [Header("Debug")]
    [Range(0, 1)]
    public float Intensity;

    private TweetComponent tweetComponent;

    void Start ()
	{
	    akAmbient = GetComponent<AkAmbient>();
	    lightRenderer = GetComponentInChildren<Renderer>();

        // TODO: Pooling
	    // particleSystem = GetComponentInChildren<ParticleSystem>(true);

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

        bool triggeredThisFrame = Trigger;
	    if (Trigger)
	    {
	        timeSinceLastTrigger = 0;
	        Trigger = false;
        }
	    timeSinceLastTrigger += Time.deltaTime;

	    if (tweetComponent == null)
	    {
	        tweetComponent = GetComponent<TweetComponent>();
	    }

	    switch (tweetComponent.State)
	    {
            case TweetComponent.TweetState.Idle:
                if (triggeredThisFrame)
                {
                    if (smoothLightCoroutine != null)
                    {
                        StopCoroutine(smoothLightCoroutine);
                    }

                    smoothLightCoroutine = StartCoroutine(smoothLight(FadeType.FadeInOut, null));

                    /*
                    if (akAmbient != null)
                    {
                        AkSoundEngine.PostEvent((uint)akAmbient.eventID, akAmbient.gameObject);
                    }
                    */
                }
                break;

            case TweetComponent.TweetState.TakingOff:
            case TweetComponent.TweetState.Approaching:
                if (smoothLightCoroutine != null)
                {
                    StopCoroutine(smoothLightCoroutine);
                }
                setIntensity(1.5f);
                break;

            case TweetComponent.TweetState.LightingUp:
                break;

            case TweetComponent.TweetState.Spawning:
                if (smoothLightCoroutine == null)
                {
                    smoothLightCoroutine = StartCoroutine(smoothLight(FadeType.FadeOut, null));
                }
                else if (currentFadeType != FadeType.FadeOut)
                {
                    StopCoroutine(smoothLightCoroutine);
                    smoothLightCoroutine = StartCoroutine(smoothLight(FadeType.FadeOut, null));
                }
                break;

            case TweetComponent.TweetState.FadingOut:
            case TweetComponent.TweetState.Returning:
                break;

            case TweetComponent.TweetState.Finished:
                if (Intensity < 0.1f)
                {
                    Destroy(gameObject);
                }
                else if (smoothLightCoroutine == null)
                {
                    smoothLightCoroutine = StartCoroutine(smoothLight(FadeType.FadeOut, () => { Destroy(gameObject); }));
                }
                else if (currentFadeType != FadeType.FadeOut)
                {
                    StopCoroutine(smoothLightCoroutine);
                    smoothLightCoroutine = StartCoroutine(smoothLight(FadeType.FadeOut, () => { Destroy(gameObject); }));
                }
                break;
	    }
	}

    IEnumerator smoothLight(FadeType fadeType, Action onFinished)
    {
        currentFadeType = fadeType;
        float t = fadeType == FadeType.FadeOut ? EaseInDuration + SustainDuration : 0;

        while (t <= EaseInDuration + SustainDuration + EaseOutDuration)
        {
            float intensity = MaxIntensity;
            if (t < EaseInDuration)
            {
                intensity = MinIntensity + Mathf.Pow(t / EaseInDuration, 2) * (MaxIntensity - MinIntensity);
            }
            else if (t >= EaseInDuration + SustainDuration)
            {
                intensity = MinIntensity + Mathf.Pow(1 - (t - EaseInDuration - SustainDuration) / EaseOutDuration, 2) * (MaxIntensity - MinIntensity);
            }

            setIntensity(intensity);

            t += Time.deltaTime;
            yield return null;
        }

        setIntensity(MinIntensity);
        smoothLightCoroutine = null;

        if (onFinished != null)
        {
            onFinished();
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
