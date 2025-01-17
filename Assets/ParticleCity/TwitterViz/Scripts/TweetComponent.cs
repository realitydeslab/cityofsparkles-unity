using System.Collections;
using System.Collections.Generic;
using TMPro;
using TwitterViz.DataModels;
using UnityEngine;
using WanderUtils;
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
        
        /// <summary>
        /// Accelerate, flying towards a hand
        /// </summary>
        TakingOff,

        /// <summary>
        /// Flying towards a hand
        /// </summary>
        Approaching,

        /// <summary>
        /// Triggered, flying towards the first word
        /// </summary>
        LightingUp,

        Holding,

        /// <summary>
        /// Spawning words
        /// </summary>
        Spawning,

        /// <summary>
        /// Words are fading out
        /// </summary>
        FadingOut,

        /// <summary>
        /// Not used
        /// </summary>
        Returning,

        /// <summary>
        /// Lifecycle finished and should be destroyed
        /// </summary>
        Finished,
    }

    public enum NodeRoleType
    {
        Invalid = 0,
        Tweet,
        StoryTrigger,
        RandomSpotTweet
    }

    public NodeRoleType NodeRole = NodeRoleType.Tweet;

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
    public bool ExplodeEffect = false;

    [Header("Animation Rising")]
    public float TargetOffset = 20;
    public float FadeInDuration = 0.3f;
    public float RisingDuration = 1.0f;
    public float FadeOutDuration = 0.2f;

    [Header("Animation Circular")] 
    public float CircularSpawnOffset = 15;
    public float CircularRisingOffset = 5;
    public float CircularRadius = 50;
    public float CircularFadeInDuration = 0.3f;
    public float CircularRisingDuration = 0.5f;
    public float CircularWordInterval = 0.2f;
    public float CircularFadeOutDuration = 2.0f;
    public float CircularSpaceWidth = 5;
    // public float CircularMaxDegree = 90;

    [Header("Word holding for random tweets")]
    public float HoldingTimeOnGaze = 2;
    public float HoldingTimeNoGaze = 5;

    [Header("Sound")]
    public string AkEventOnReveal = "Play_TweetRevealCommon";

    [Header("Tutorials")] 
    public GameObject SpawnTweetTutorialLeftPrefab;
    public GameObject SpawnTweetTutorialRightPrefab;
    public float SpawnTweetWaitTimeBeforeTutorial = 5;
    private GameObject spawnTweetTutorial;
    private float spawnTweetWaitedTime = 0;

    [Header("Debug")]
    public bool GrabPlayer;
    public TweetState State;
    [TextArea]
    public string Text;
    [Range(-1, 1)]
    public double Sentiment;
    public Camera FacingCamera;

    public SpawnSourceNode SpawnSource;
    public object SpawnSourceUserData;

    private AkGameObj akGameObj;
    private bool everTriggered;

    private Transform approachingTarget;
    private float stateChangeTime;
    private float speedLimit;
    public GuidingLight guidingLight { get; private set; }
    private WindZone windZone;

    private bool addedToActiveList = false;

    void Awake()
    {
        akGameObj = GetComponent<AkGameObj>();
    }

	void Start ()
	{
	    guidingLight = GetComponentInChildren<GuidingLight>();
	    windZone = GetComponentInChildren<WindZone>(true);

	    GameObject captureGameObject = GameObject.FindWithTag("Capture");
	    if (captureGameObject != null)
	    {
	        FacingCamera = captureGameObject.GetComponent<Camera>();
	    }
	}
	
	void Update () {
	    switch (State)
	    {
            case TweetState.Idle:
                if (Trigger)
                {
                    Trigger = false;
                    if (!everTriggered)
                    {
                        if (SpawnSource != null)
                        {
                            SpawnSource.OnTweetTriggered(this);
                        }
                    }
                    everTriggered = true;

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

            case TweetState.Finished:
                if (guidingLight == null || !guidingLight.enabled)
                {
                    Destroy(gameObject);
                }
                break;
	    }

        // Debug
	    if (GrabPlayer)
	    {
	        GrabPlayer = false;
	        Transform player = InputManager.Instance.PlayerTransform;
	        Transform head = InputManager.Instance.CenterCamera.transform;
	        Vector3 headMove = transform.position - head.forward * 100 - head.position;
	        player.position += headMove;
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
	    // }
	}

    public void Finish()
    {
        setState(TweetState.Finished);
    }

    public bool IsTriggered
    {
        get
        {
            switch (State)
            {
                case TweetComponent.TweetState.Idle:
                case TweetComponent.TweetState.TakingOff:
                case TweetComponent.TweetState.Approaching:
                    return false;

                default:
                    return true;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (State == TweetState.Idle && InputManager.Instance.IsActiveHand(other.gameObject))
        {
            Trigger = true;
            approachingTarget = other.transform;
        }

        if (TutorialStateManager.Instance.State == TutorialState.InitialRedDot &&
            InputManager.Instance.IsActiveHand(other.gameObject))
        {
            TutorialStateManager.Instance.InitialRedDotCloseEnough();
        }
    }

    void OnTriggerExit(Collider other)
    {
        // if ( (State == TweetState.TakingOff || State == TweetState.Approaching) &&
        //      approachingTarget == other.transform)
        // {
        //     approachingTarget = null;
        //     setState(TweetState.Returning);
        // }
    }

    void OnDestroy()
    {
        if (addedToActiveList)
        {
            ParticleCity.Current.RemoveActiveGameObject(gameObject);
            addedToActiveList = false;
        }

        if (SpawnSource != null)
        {
            SpawnSource.OnTweetDestroy(this);
        }
    }

    private IEnumerator wordAnimation()
    {
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
            StartCoroutine(wordFadeOut(tmpText, FadeOutDuration, i == 0, i == Tweet.Words.Length - 1));
        }
    }

    private void updateRenderPart(Vector3 pos, Vector3 velocity)
    {
        guidingLight.RenderPart.position = pos;
        guidingLight.RenderPart.forward = velocity.normalized;
        windZone.transform.forward = pos - transform.position;
    }

    private IEnumerator wordAnimationCircular()
    {
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
        Vector3 cameraPos = (FacingCamera != null) ? FacingCamera.transform.position : Camera.main.transform.position;
        Vector3 cameraToPointDir = (transform.position - cameraPos);
        cameraToPointDir.y = CircularSpawnOffset / CircularRadius * Mathf.Sqrt(cameraToPointDir.x * cameraToPointDir.x + cameraToPointDir.z * cameraToPointDir.z);
        cameraToPointDir.Normalize();
        Debug.DrawLine(cameraPos, transform.position, Color.yellow, 5);
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
        // guidingLight.LightUpForSpawning();
        ParticleCity.Current.AddActiveGameObject(guidingLight.RenderPart.gameObject);
        addedToActiveList = true;

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
            updateRenderPart(newPos, lightVelocity);
            yield return null;
        }

        // Animation
        ParticleCity.Current.RemoveActiveGameObject(guidingLight.RenderPart.gameObject, 1);
        addedToActiveList = false;
        setState(TweetState.Spawning);
        playMusic();
        Vector3 lightTargetPos = transform.position;
        for (int i = 0; i < textObjects.Count; i++)
        {
            bool isFirst = (i == 0);
            bool isLast = (i == textObjects.Count - 1);
            TMP_Text text = textObjects[i];
            lightTargetPos = (i == textObjects.Count - 1) ? text.transform.position : textObjects[i + 1].transform.position;

            Vector3 originLocal = text.transform.localPosition;
            originLocal.y = CircularSpawnOffset;
            Vector3 targetLocal = originLocal + new Vector3(0, CircularRisingOffset, 0);

            StartCoroutine(circularWordFadeIn(text, isFirst, isLast, originLocal, targetLocal));

            float time = 0;
            while (time < CircularWordInterval)
            {
                // guidingLight.RenderPart.position = Vector3.Lerp(guidingLight.RenderPart.position, lightTargetPos, SpawningLerpRatio * Time.deltaTime);
                Vector3 pos = Vector3.SmoothDamp(guidingLight.RenderPart.position, lightTargetPos, ref lightVelocity, Mathf.Max(CircularRisingDuration, CircularFadeInDuration));
                updateRenderPart(pos, lightVelocity);
                time += Time.deltaTime;

                yield return null;
            }
        }

        setState(TweetState.FadingOut);

        // Keep flying the light until it dies
        while ((lightTargetPos - guidingLight.RenderPart.position).sqrMagnitude > 0.01f)
        {
            Vector3 pos = Vector3.SmoothDamp(guidingLight.RenderPart.position, lightTargetPos, ref lightVelocity, Mathf.Max(CircularRisingDuration, CircularFadeInDuration));
            updateRenderPart(pos, lightVelocity);
            yield return null;
        }
    }

    private IEnumerator wordAnimationOneSpot()
    {
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
        Vector3 cameraPos = (FacingCamera != null) ? FacingCamera.transform.position : Camera.main.transform.position;
        Vector3 cameraToPointDir = (transform.position - cameraPos);
        cameraToPointDir.y = CircularSpawnOffset / CircularRadius * Mathf.Sqrt(cameraToPointDir.x * cameraToPointDir.x + cameraToPointDir.z * cameraToPointDir.z);
        cameraToPointDir.Normalize();
        Debug.DrawLine(cameraPos, transform.position, Color.yellow, 5);
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

        // Animation
        setState(TweetState.Spawning);
        playMusic();
        for (int i = 0; i < textObjects.Count; i++)
        {
            bool isFirst = (i == 0);
            bool isLast = (i == textObjects.Count - 1);
            TMP_Text text = textObjects[i];

            Vector3 originLocal = Vector3.zero;
            Vector3 targetLocal = textObjects[i].transform.localPosition + new Vector3(0, CircularSpawnOffset + CircularRisingOffset, 0);
            StartCoroutine(circularWordFadeIn(text, isFirst, isLast, originLocal, targetLocal, false));

            yield return new WaitForSeconds(CircularWordInterval);
        }

        setState(TweetState.Holding);

        float holdingTime = 0;
        float gazeTime = 0;
        while (State == TweetState.Holding)
        {
            holdingTime += Time.deltaTime;
            if (holdingTime > HoldingTimeNoGaze)
            {
                setState(TweetState.FadingOut);
            }
            else
            {
                float angle = Vector3.Angle(InputManager.Instance.CenterCamera.transform.forward, cameraToPointDir);
                if (angle < 45)
                {
                    gazeTime += Time.deltaTime;
                }
                else
                {
                    gazeTime = 0;
                }

                if (gazeTime > HoldingTimeOnGaze)
                {
                    setState(TweetState.FadingOut);
                }
            }
            yield return null;
        }

        for (int i = 0; i < textObjects.Count; i++)
        {
            bool isFirst = (i == 0);
            bool isLast = (i == textObjects.Count - 1);
            TMP_Text text = textObjects[i];

            StartCoroutine(wordFadeOut(text, CircularFadeOutDuration, isFirst, isLast));
            ParticleCity.Current.RemoveActiveGameObject(text.gameObject, 1);

            yield return new WaitForSeconds(CircularWordInterval);
        }
    }

    private IEnumerator circularWordFadeIn(TMP_Text text, bool isFirst, bool isLast, Vector3 originLocal, Vector3 targetLocal, bool fadeOut = true)
    {
        ParticleCity.Current.AddActiveGameObject(text.gameObject);

        float time = 0;
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

            // Vector3 offset = text.transform.localPosition;
            if (time < CircularRisingDuration)
            {
                // offset.y = CircularSpawnOffset + Mathf.SmoothStep(0, CircularRisingOffset, time / CircularRisingDuration);
                text.transform.localPosition = Vector3.Lerp(originLocal, targetLocal, Mathf.SmoothStep(0, 1, time / CircularRisingDuration));
            }
            else
            {
                // offset.y = CircularSpawnOffset + CircularRisingOffset;
                text.transform.localPosition = targetLocal;
            }
            // text.transform.localPosition = offset;

            akGameObj.m_positionOffsetData = new AkGameObjPositionOffsetData()
            {
                positionOffset = text.transform.localPosition
            };

            time += Time.deltaTime;

            yield return null;
        }

        text.transform.localPosition = targetLocal;

        if (fadeOut)
        {
            StartCoroutine(wordFadeOut(text, CircularFadeOutDuration, isFirst, isLast));
            ParticleCity.Current.RemoveActiveGameObject(text.gameObject, 1);
        }
    }

    private IEnumerator storyTriggerAnimation()
    {
        if (SpawnSource == null || SpawnSource.Next == null || SpawnSource.Next.Count == 0)
        {
            Debug.LogWarning("No SpawnSource or Next node defined on StoryTrigger node. ", this);
            Finish();
            yield break;
        }

        // Lighting up
        Vector3 lightVelocity = Vector3.zero;
        ParticleCity.Current.AddActiveGameObject(guidingLight.RenderPart.gameObject);
        addedToActiveList = true;

        StoryNode next = SpawnSource.Next[0];
        Vector3 startingPoint = next.gameObject.transform.position;
        while ((startingPoint - guidingLight.RenderPart.position).sqrMagnitude > SpawnStartingDistanceThreshold * SpawnStartingDistanceThreshold)
        {
            Vector3 newPos = Vector3.Lerp(guidingLight.RenderPart.position, startingPoint, SpawningLerpRatio * Time.deltaTime);
            lightVelocity = (newPos - guidingLight.RenderPart.position) / Time.deltaTime;
            if (lightVelocity.sqrMagnitude > LightUpMaxSpeed * LightUpMaxSpeed)
            {
                lightVelocity = lightVelocity.normalized * LightUpMaxSpeed;
                newPos = guidingLight.RenderPart.position + lightVelocity * Time.deltaTime;
            }
            updateRenderPart(newPos, lightVelocity);
            yield return null;
        }

        SpawnSource.OnTweetRevealed(this);

        ParticleCity.Current.RemoveActiveGameObject(guidingLight.RenderPart.gameObject, 1);
        addedToActiveList = false;
        Destroy(gameObject);
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
            HandType handType = InputManager.Instance.GetHandType(approachingTarget);
            spawnTweetWaitedTime += Time.deltaTime;

            if ((TutorialStateManager.Instance.IsInInitialTutorialSteps || spawnTweetWaitedTime > SpawnTweetWaitTimeBeforeTutorial) && spawnTweetTutorial == null && handType != HandType.Unknown)
            {
                // GameObject prefab = (handType == HandType.Left) ? SpawnTweetTutorialLeftPrefab : SpawnTweetTutorialRightPrefab;
                // spawnTweetTutorial = Instantiate(prefab);
                // spawnTweetTutorialShowOnce = true;
                TutorialStateManager.Instance.RedDotReached();
            }

            if (
                handType != HandType.Unknown && (
                InputManager.Instance.GetGrabDown(HandType.Left) ||
                InputManager.Instance.GetGrabDown(HandType.Right) ||
                InputManager.Instance.GetButtonDown(Button.Confirm) ))
            {
                TutorialStateManager.Instance.TwitterTriggered();
                if (spawnTweetTutorial != null)
                {
                    Destroy(spawnTweetTutorial);
                    spawnTweetWaitedTime = 0;
                }

                setState(TweetState.LightingUp);
            }
            // setState(TweetState.LightingUp);

            // if (Tweet.Words.Length > 0)
            // {
            //     setState(TweetState.LightingUp);
            // }
            // else
            // {
            //     setState(TweetState.Returning);
            // }
        }
    }

    private IEnumerator revealTweetAnimations()
    {
        switch (NodeRole)
        {
            case NodeRoleType.Tweet:
            default:
                StartCoroutine(wordAnimationCircular());
                break;

            case NodeRoleType.StoryTrigger:
                StartCoroutine(storyTriggerAnimation());
                break;

            case NodeRoleType.RandomSpotTweet:
                StartCoroutine(wordAnimationOneSpot());
                break;
        }

        if (ExplodeEffect)
        {
            ParticleSystem particle = GetComponentInChildren<ParticleSystem>(true);
            particle.gameObject.SetActive(true);
            particle.Play();

            // guidingLight.TurnOff(true);

            yield return new WaitForSeconds(0.1f);


            windZone.gameObject.SetActive(true);
        }
    }

    private IEnumerator returnAnimation()
    {
        yield return null;
    }

    private IEnumerator wordFadeOut(TMP_Text word, float duration, bool isFirst, bool isLast)
    {
        float time = 0;
        while (time < duration)
        {
            word.alpha = 1 - time / duration;
            time += Time.deltaTime;
            yield return null;
        }

        Destroy(word.gameObject);

        if (isFirst && SpawnSource != null)
        {
            SpawnSource.OnTweetRevealed(this);
        }

        if (isLast)
        {
            Finish();
        }
    }

    private void playMusic()
    {
        int length = (Tweet == null || Tweet.Words == null) ? 0 : Tweet.Words.Length;
        AkSoundEngine.SetRTPCValue("TweetWordsLength", length, gameObject);
        AkSoundEngine.SetRTPCValue("TweetSentiment", (float)Sentiment, gameObject);
        AkSoundEngine.PostEvent(AkEventOnReveal, gameObject);
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
        Renderer [] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(renderers[0].transform.position, LightUpDistanceThreshold);
        }
    }
}
