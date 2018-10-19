using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SplashSlideshow : MonoBehaviour
{
    public Sprite[] Slides;
    public float Interval = 10;
    public float FadeTime = 2;
    public float Scale = 1.1f;

    private Image Panel1;
    private Image Panel2;
    private int currentIndex;
    private float scaleFrom;
    private float scaleTo;

    private float lastSwitchTime;

    void Start()
    {
        Image[] panels = GetComponentsInChildren<Image>();
        Panel1 = panels[0];
        Panel2 = panels[1];

        currentIndex = 0;
        scaleFrom = 1;
        scaleTo = Scale;

        Panel1.sprite = Slides[0];
        Panel1.type = Image.Type.Simple;
        Panel1.color = Color.white;

        Panel2.sprite = null;
        Panel2.type = Image.Type.Simple;
        Panel2.color = Color.clear;

        lastSwitchTime = Time.time;
    }


    void Update()
    {
        if (Time.time - lastSwitchTime > Interval)
        {
            StartCoroutine(gotoNext());
            lastSwitchTime = Time.time;
        }

        Panel1.transform.localScale += Vector3.one * (scaleTo - scaleFrom) / Interval * Time.deltaTime;
    }

    private IEnumerator gotoNext()
    {
        int nextIndex = (currentIndex + 1) % Slides.Length;
        Panel2.sprite = Slides[nextIndex];
        Panel2.color = Color.clear;
        Panel2.transform.localScale = Vector3.one * scaleTo;

        float t = 0;
        while (t < FadeTime)
        {
            Panel1.color = Color.Lerp(Color.white, Color.clear, t / FadeTime);
            Panel2.color = Color.Lerp(Color.clear, Color.white, t / FadeTime);
            Panel2.transform.localScale += Vector3.one * (scaleFrom - scaleTo) / Interval * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }

        Image temp = Panel1;
        Panel1 = Panel2;
        Panel2 = temp;
        currentIndex = nextIndex;

        float scaleTemp = scaleFrom;
        scaleFrom = scaleTo;
        scaleTo = scaleTemp;

        Panel2.transform.localScale = Vector3.one;
    }
}
