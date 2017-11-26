using System.Collections;
using System.Collections.Generic;
using TMPro;
using TwitterViz.DataModels;
using UnityEngine;

public class TweetComponent : MonoBehaviour
{
    public TMP_Text WordPrefab;
    public bool Trigger;
    public Tweet Tweet;

    [Header("Animation 1")]
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

	void Start () {
		
	}
	
	void Update () {
	    if (Trigger)
	    {
	        Trigger = false;
	        if (!isPlaying)
	        {
	            StartCoroutine(wordAnimationCircular());
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
        Vector3 cameraToPointDir = (transform.position - Camera.main.transform.position).normalized;
        Debug.DrawLine(Camera.main.transform.position, transform.position, Color.yellow, 5);
        Debug.DrawRay(transform.position, cameraToPointDir * CircularRadius, Color.blue, 5);

        cameraToPointDir.y = 0;
        float totalWidth = CircularSpaceWidth * (textObjects.Count - 1);
        for (int i = 0; i < textObjects.Count; i++)
        {
            totalWidth += textObjects[i].bounds.extents.x * 2;
        }
        float totalDegree = totalWidth / (CircularRadius * Mathf.PI * 2) * 360.0f;
        float accumulatedWidth = 0;
        for (int i = 0; i < textObjects.Count; i++)
        {
            accumulatedWidth += textObjects[i].bounds.extents.x;
            float currentDegree = (accumulatedWidth / totalWidth - 0.5f) * totalDegree;
            Vector3 pointToWordDir = Quaternion.Euler(0, currentDegree, 0) * cameraToPointDir;
            Debug.DrawRay(transform.position, pointToWordDir * CircularRadius, Color.red, 5);

            Vector3 localPosition = CircularRadius * pointToWordDir;
            textObjects[i].transform.localPosition = localPosition;
            textObjects[i].transform.forward = pointToWordDir;

            accumulatedWidth += textObjects[i].bounds.extents.x + CircularSpaceWidth;
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
                    offset.y = Mathf.SmoothStep(0, TargetOffset, time / CircularRisingDuration);
                }
                else
                {
                    offset.y = TargetOffset;
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
}
