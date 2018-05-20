using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TitleNode : StoryNode
{
    public float HeightOffset;
    public float RisingDuration;
    public float FadeInDuration;
    public float HoldDuration;
    public float FadeOutDuration;

    private TextMeshPro text;

    public override void Start()
    {
        base.Start();
        text = GetComponentInChildren<TextMeshPro>();
        StartCoroutine(fadeInOut());
        StartCoroutine(rising());
    }

    private IEnumerator fadeInOut()
    {
        yield return null;
    }

    private IEnumerator rising()
    {
        yield return null;
    }
}
