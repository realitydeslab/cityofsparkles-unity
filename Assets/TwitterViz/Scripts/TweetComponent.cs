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
    public float TargetOffset = 20;

    public float FadeInDuration = 0.3f;
    public float RisingDuration = 1.0f;
    public float FadeOutDuration = 0.2f;

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
	            StartCoroutine(wordAnimation());
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
            StartCoroutine(wordFadeOut(tmpText));
        }

        isPlaying = false;
    }

    private IEnumerator wordFadeOut(TMP_Text word)
    {
        float time = 0;
        while (time < FadeOutDuration)
        {
            word.alpha = 1 - time / FadeOutDuration;
            time += Time.deltaTime;
            yield return null;
        }
        Destroy(word.gameObject);
    }
}
