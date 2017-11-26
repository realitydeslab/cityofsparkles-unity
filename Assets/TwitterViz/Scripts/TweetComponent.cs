using System.Collections;
using System.Collections.Generic;
using TMPro;
using TwitterViz.DataModels;
using UnityEngine;

public class TweetComponent : MonoBehaviour
{
    public enum SpawnAnimation
    {
        Unspecified = 0,
        Rising,
        Circular
    }

    public TMP_Text WordPrefab;
    public bool Trigger;
    public Tweet Tweet;
    public SpawnAnimation Animation;

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

    [Header("Debugging")]
    [TextArea]
    public string Text;

    [Range(-1, 1)]
    public double Sentiment;

    private bool isPlaying;
    private AkAmbient akAmbient;

    void Awake()
    {
        akAmbient = GetComponent<AkAmbient>();
    }

	void Start () {
	}
	
	void Update () {
	    if (Trigger)
	    {
	        Trigger = false;

            if (Animation == SpawnAnimation.Unspecified)
            {
                Animation = UnityEngine.Random.value < 0.2f ? SpawnAnimation.Rising : SpawnAnimation.Circular;
            }

            if (!isPlaying)
	        {
	            switch (Animation)
	            {
	                case SpawnAnimation.Rising:
	                    playMusic();
                        StartCoroutine(wordAnimation());
	                    break;

                    case SpawnAnimation.Circular:
	                    playMusic();
                        StartCoroutine(wordAnimationCircular());
                        break;

                    default:
	                    break;
	            }
            }
	    }	
	}

    void OnTriggerEnter()
    {
        Trigger = true;
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

        // Animation
        for (int i = 0; i < textObjects.Count; i++)
        {
            TMP_Text text = textObjects[i];
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

                time += Time.deltaTime;

                yield return null;
            }
            StartCoroutine(wordFadeOut(text, CircularFadeOutDuration));
        }

        isPlaying = false;
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
}
