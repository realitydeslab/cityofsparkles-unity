using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SplashSlideshow : MonoBehaviour
{
    public Sprite[] Slides;
    public float Interval = 10;
    public float FadeTime = 2;

    private Image Panel1;
    private Image Panel2;
    private int currentIndex;

    private float lastSwitchTime;

    void Start()
    {
        Image[] panels = GetComponentsInChildren<Image>();
        Panel1 = panels[0];
        Panel2 = panels[1];

        currentIndex = 0;

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
    }

    private IEnumerator gotoNext()
    {
        int nextIndex = (currentIndex + 1) % Slides.Length;
        Panel2.sprite = Slides[nextIndex];
        Panel2.color = Color.clear;

        float t = 0;
        while (t < FadeTime)
        {
            Panel1.color = Color.Lerp(Color.white, Color.clear, t / FadeTime);
            Panel2.color = Color.Lerp(Color.clear, Color.white, t / FadeTime);
            t += Time.deltaTime;
            yield return null;
        }

        Image temp = Panel1;
        Panel1 = Panel2;
        Panel2 = temp;
        currentIndex = nextIndex;
    }
}
