using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteMusicSync : MonoBehaviour
{
    public int FlickerPerCycle = 2;

    [Header("Auto")] 
    public float EaseInDuration;
    public float SustainDuration;
    public float EaseOutDuration;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        InteractiveMusicController.Instance.AkMusicSyncCueTriggered += OnAkMusicSyncCueTriggered;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        
    }

    void OnDestroy()
    {
        if (InteractiveMusicController.Instance != null)
        {
            InteractiveMusicController.Instance.AkMusicSyncCueTriggered -= OnAkMusicSyncCueTriggered;
            InteractiveMusicController.Instance.RemovePointOfInterest(gameObject);
        }
    }

    private void OnAkMusicSyncCueTriggered(string cue)
    {
        string[] comp = cue.Split(':');
        EaseInDuration = float.Parse(comp[1]);
        SustainDuration = float.Parse(comp[2]);
        EaseOutDuration = float.Parse(comp[3]);

        spriteRenderer.enabled = true;
        StartCoroutine(flicker(EaseInDuration + SustainDuration + EaseOutDuration));
    }

    private IEnumerator flicker(float totalLifeTime)
    {
        float interval = totalLifeTime / (FlickerPerCycle * 2);
        for (int i = 1; i < FlickerPerCycle * 2; i++)
        {
            yield return new WaitForSeconds(interval);
            spriteRenderer.enabled = !spriteRenderer.enabled;
        }
        spriteRenderer.enabled = false;
    }
}
