using System.Collections;
using System.Collections.Generic;
using ParticleCities;
using TMPro;
using TwitterViz.DataModels;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class TweetComponent : MonoBehaviour
{
    public enum SpawnAnimation
    {
        Unspecified = 0,
        Rising,
        Circular
    }

    public enum TweetState
    {
        Idle = 0,
        
        TakingOff,
        Approaching,

        LightingUp,
        Spawning,
        FadingOut,

        Returning,
    }

    public TMP_Text WordPrefab;
    public bool Trigger;
    public Tweet Tweet;
    public SpawnAnimation Animation;

    [Header("Tweet Motion")] 
    public float TakingOffDuration = 1;
    public float ApproachingMaxSpeed = 1;
    public float ApproachingLerpRatio = 0.5f;
    public float LightUpMaxSpeed = 2;
    public float LightUpDistanceThreshold = 0.2f;
    public float SpawnStartingDistanceThreshold = 0.2f;
    public float SpawningLerpRatio = 10f;

    [Header("Animation Rising")]
    public float TargetOffset = 20;
    public float FadeInDuration = 0.3f;
    public float RisingDuration = 1.0f;
    public float FadeOutDuration = 0.2f;

    [Header("Animation Circular")] 
    public float CircularOffset = 20;
    public float CircularRadius = 50;
    public float CircularFadeInDuration = 0.3f;
    public float CircularRisingDuration = 0.5f;
    public float CircularFadeOutDuration = 2.0f;
    public float CircularSpaceWidth = 5;
    // public float CircularMaxDegree = 90;

    [Header("Debug")]
    public bool GrabPlayer;
    public TweetState State;
    [TextArea]
    public string Text;
    [Range(-1, 1)]
    public double Sentiment;

    public TwitterManager.Sentiment TargetSentiment;

    private Vector3 originalPosition;
    private bool isPlaying;
    private AkAmbient akAmbient;
    private AkGameObj akGameObj;
    private bool everTriggered;
    private TwitterManager manager;

    private Transform approachingTarget;
    private float stateChangeTime;
    private float speedLimit;
    private GuidingLight guidingLight;

    void Awake()
    {
        akAmbient = GetComponent<AkAmbient>();
        akGameObj = GetComponent<AkGameObj>();
    }

	void Start ()
	{
	    manager = GetComponentInParent<TwitterManager>();
	    guidingLight = GetComponentInChildren<GuidingLight>();

	    originalPosition = transform.position;
	}
	
	void Update () {
	    switch (State)
	    {
            case TweetState.Idle:
                if (Trigger)
                {
                    Trigger = false;
                    if (approachingTarget != null)
                    {
                        setState(TweetState.TakingOff);
                    }
                    else
                    {
                        setState(TweetState.LightingUp);
                    }
                }
                break;

            case TweetState.TakingOff:
                updateTakingOff();
                break;

            case TweetState.Approaching:
                updateApproaching();
                break;

            case TweetState.LightingUp:
            case TweetState.Spawning:
            case TweetState.FadingOut:
                // Coroutine happening
                break;

            case TweetState.Returning:
                // Coroutine happening
                break;
	    }

        // Debug
	    if (GrabPlayer)
	    {
	        GrabPlayer = false;
	        Transform player = InputManager.Instance.PlayerTransform;
	        player.position = transform.position - (player.forward) * 100;
	        player.forward = (transform.position - player.position);
	    }

	    // if (Trigger)
	    // {
	    //     Trigger = false;

	    //     if (!everTriggered)
	    //     {
	    //         manager.RecordFirstTrigger(this);    
	    //     }
	    //     everTriggered = true;

        //     if (Animation == SpawnAnimation.Unspecified)
        //     {
        //         Animation = UnityEngine.Random.value < 0.2f ? SpawnAnimation.Rising : SpawnAnimation.Circular;
        //     }

        //     if (!isPlaying)
	    //     {
	    //         switch (Animation)
	    //         {
	    //             case SpawnAnimation.Rising:
	    //                 playMusic();
        //                 StartCoroutine(wordAnimation());
	    //                 break;

        //             case SpawnAnimation.Circular:
	    //                 playMusic();
        //                 StartCoroutine(wordAnimationCircular());
        //                 break;

        //             default:
	    //                 break;
	    //         }
        //     }
	    // }	
	}

    void OnTriggerEnter(Collider other)
    {
        if (State == TweetState.Idle)
        {
            Trigger = true;
            approachingTarget = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if ( (State == TweetState.TakingOff || State == TweetState.Approaching) &&
             approachingTarget == other.transform)
        {
            approachingTarget = null;
            setState(TweetState.Returning);
        }
    }

    public void MarkForDestroy()
    {
        GuidingLight light = GetComponent<GuidingLight>();
        if (light != null)
        {
            light.MarkForDestroy();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator wordAnimation()
    {
        isPlaying = true;

        for (int i = 0; i < Tweet.Words.Length; i++)
        {
            TMP_Text tmpText = Instantiate(WordPrefab, transform, false);
            tmpText.transform.forward = Camera.main.transform.forward;
            tmpText.text = Tweet.Words[i];

            Vector3 cameraToPoint = transform.position - Camera.main.transform.position;
            cameraToPoint.y = 0;
            cameraToPoint.Normalize();
            tmpText.transform.localPosition += cameraToPoint * 10;

            float time = 0;
            while (time < FadeInDuration || time < RisingDuration)
            {
                if (time < FadeInDuration)
                {
                    tmpText.alpha = time / FadeInDuration;
                }
                else
                {
                    tmpText.alpha = 1;
                }

                Vector3 offset = tmpText.transform.localPosition;
                if (time < RisingDuration)
                {
                    offset.y = Mathf.SmoothStep(0, TargetOffset, time / RisingDuration);
                }
                else
                {
                    offset.y = TargetOffset;
                }
                tmpText.transform.localPosition = offset;

                time += Time.deltaTime;

                yield return null;
            }
            StartCoroutine(wordFadeOut(tmpText, FadeOutDuration));
        }

        isPlaying = false;
    }

    private IEnumerator wordAnimationCircular()
    {
        isPlaying = true;

        List<TMP_Text> textObjects = new List<TMP_Text>();
        for (int i = 0; i < Tweet.Words.Length; i++)
        {
            TMP_Text text = Instantiate(WordPrefab, transform, false);
            text.text = Tweet.Words[i];
            text.alpha = 0;
            textObjects.Add(text);
        }

        // Wait for one frame for TMP's layout
        yield return null;

        // Circular layout
        Vector3 cameraToPointDir = (transform.position - Camera.main.transform.position);
        cameraToPointDir.y = 0;
        cameraToPointDir.Normalize();
        Debug.DrawLine(Camera.main.transform.position, transform.position, Color.yellow, 5);
        Debug.DrawRay(transform.position, cameraToPointDir * CircularRadius, Color.blue, 5);

        float totalWidth = CircularSpaceWidth * (textObjects.Count - 1);
        for (int i = 0; i < textObjects.Count; i++)
        {
            totalWidth += textObjects[i].preferredWidth;
        }
        float totalDegree = totalWidth / (CircularRadius * Mathf.PI * 2) * 360.0f;
        float accumulatedWidth = 0;
        for (int i = 0; i < textObjects.Count; i++)
        {
            accumulatedWidth += textObjects[i].preferredWidth / 2;
            float currentDegree = (accumulatedWidth / totalWidth - 0.5f) * totalDegree;
            Vector3 pointToWordDir = Quaternion.Euler(0, currentDegree, 0) * cameraToPointDir;
            Debug.DrawRay(transform.position, pointToWordDir * CircularRadius, Color.red, 5);

            Vector3 localPosition = CircularRadius * pointToWordDir;
            textObjects[i].transform.localPosition = localPosition;
            textObjects[i].transform.forward = pointToWordDir;

            accumulatedWidth += textObjects[i].preferredWidth / 2 + CircularSpaceWidth;
        }

        // Lighting up
        Vector3 lightVelocity = Vector3.zero;
        guidingLight.LightUpForSpawning();
        ParticleCity.Instance.AddActiveGameObject(guidingLight.RenderPart.gameObject);

        Vector3 startingPoint = textObjects[0].transform.position;
        while ((startingPoint - guidingLight.RenderPart.position).sqrMagnitude > SpawnStartingDistanceThreshold * SpawnStartingDistanceThreshold)
        {
            Vector3 newPos = Vector3.Lerp(guidingLight.RenderPart.position, startingPoint, SpawningLerpRatio * Time.deltaTime);
            lightVelocity = (newPos - guidingLight.RenderPart.position) / Time.deltaTime;
            if (lightVelocity.sqrMagnitude > LightUpMaxSpeed * LightUpMaxSpeed)
            {
                lightVelocity = lightVelocity.normalized * LightUpMaxSpeed;
                newPos = guidingLight.RenderPart.position + lightVelocity * Time.deltaTime;
            }
            guidingLight.RenderPart.position = newPos;
            yield return null;
        }


        // Animation
        guidingLight.TurnOff();
        ParticleCity.Instance.RemoveActiveGameObject(guidingLight.RenderPart.gameObject, 1);
        setState(TweetState.Spawning);
        playMusic();
        Vector3 lightTargetPos = transform.position;
        for (int i = 0; i < textObjects.Count; i++)
        {
            TMP_Text text = textObjects[i];
            float time = 0;
            lightTargetPos = (i == textObjects.Count - 1) ? text.transform.position : textObjects[i + 1].transform.position;
            ParticleCity.Instance.AddActiveGameObject(text.gameObject);
            while (time < CircularFadeInDuration || time < CircularRisingDuration)
            {
                if (time < CircularFadeInDuration)
                {
                    text.alpha = time / CircularFadeInDuration;
                }
                else
                {
                    text.alpha = 1;
                }

                Vector3 offset = text.transform.localPosition;
                if (time < CircularRisingDuration)
                {
                    offset.y = Mathf.SmoothStep(0, CircularOffset, time / CircularRisingDuration);
                }
                else
                {
                    offset.y = CircularOffset;
                }
                text.transform.localPosition = offset;

                akGameObj.m_positionOffsetData = new AkGameObjPositionOffsetData()
                {
                    positionOffset = text.transform.localPosition
                };

                time += Time.deltaTime;

                // guidingLight.RenderPart.position = Vector3.Lerp(guidingLight.RenderPart.position, lightTargetPos, SpawningLerpRatio * Time.deltaTime);
                guidingLight.RenderPart.position = Vector3.SmoothDamp(guidingLight.RenderPart.position, lightTargetPos,
                    ref lightVelocity, Mathf.Max(CircularRisingDuration, CircularFadeInDuration));
                guidingLight.RenderPart.forward = lightVelocity.normalized;
                yield return null;
            }
            StartCoroutine(wordFadeOut(text, CircularFadeOutDuration));
            ParticleCity.Instance.RemoveActiveGameObject(text.gameObject, 1);
        }

        setState(TweetState.FadingOut);

        // Keep flying the light until it dies
        while ((lightTargetPos - guidingLight.RenderPart.position).sqrMagnitude > 0.01f)
        {
            guidingLight.RenderPart.position = Vector3.SmoothDamp(guidingLight.RenderPart.position, lightTargetPos,
                ref lightVelocity, Mathf.Max(CircularRisingDuration, CircularFadeInDuration));
            guidingLight.RenderPart.forward = lightVelocity.normalized;
            yield return null;
        }
        guidingLight.MarkForDestroy();

        isPlaying = false;
    }

    private void updateTakingOff()
    {
        float progress = (Time.time - stateChangeTime) / TakingOffDuration;
        if (progress > 1)
        {
            speedLimit = ApproachingMaxSpeed;
            setState(TweetState.Approaching);
        }
        else
        {
            speedLimit = Mathf.Lerp(0, ApproachingMaxSpeed, progress);
        }

        updateApproaching();
    }

    private void updateApproaching()
    {
        Vector3 newPos = Vector3.Lerp(guidingLight.RenderPart.position, approachingTarget.position, ApproachingLerpRatio * Time.deltaTime);
        Vector3 velocity = (newPos - guidingLight.RenderPart.position) / Time.deltaTime;
        if (velocity.sqrMagnitude > speedLimit * speedLimit)
        {
            newPos = guidingLight.RenderPart.position + velocity.normalized * speedLimit * Time.deltaTime;
        }
        guidingLight.transform.position = newPos;

        if ((guidingLight.RenderPart.position - approachingTarget.position).sqrMagnitude < LightUpDistanceThreshold * LightUpDistanceThreshold)
        {
            if (Tweet.Words.Length > 0)
            {
                setState(TweetState.LightingUp);
            }
            else
            {
                setState(TweetState.Returning);
            }
        }
    }

    private IEnumerator revealTweetAnimations()
    {
        yield return StartCoroutine(wordAnimationCircular());
    }

    private IEnumerator returnAnimation()
    {
        yield return null;
    }

    private IEnumerator wordFadeOut(TMP_Text word, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            word.alpha = 1 - time / duration;
            time += Time.deltaTime;
            yield return null;
        }
        Destroy(word.gameObject);
    }

    private void playMusic()
    {
        int length = Tweet.Words.Length;
        if (length < 6)
        {
            AkSoundEngine.PostEvent("Play_Tweet_Short", gameObject);
        }
        else if (length < 10)
        {
            AkSoundEngine.PostEvent("Play_Tweet_Med", gameObject);
        }
        else if (length < 20)
        {
            AkSoundEngine.PostEvent("Play_Tweet_Med_Long", gameObject);
        }
        else
        {
            AkSoundEngine.PostEvent("Play_Tweet_Long", gameObject);
        }
    }

    private void setState(TweetState newState)
    {
        State = newState;
        stateChangeTime = Time.time;

        if (newState == TweetState.LightingUp)
        {
            StartCoroutine(revealTweetAnimations());
        }
        else if (newState == TweetState.Returning)
        {
            StartCoroutine(returnAnimation());
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetComponentInChildren<Renderer>().transform.position, LightUpDistanceThreshold);
    }
}
