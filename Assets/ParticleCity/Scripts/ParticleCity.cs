using System.Collections;
using System.Collections.Generic;
using ParticleCities;
using UnityEngine;
using WanderUtils;

[RequireComponent(typeof(ParticleCityAnimator))]
public class ParticleCity : MonoBehaviour
{
    public static ParticleCity Current
    {
        get { return StageSwitcher.Instance.CurrentParticleCity; }
    }

    public List<GameObject> ActiveGameObjects = new List<GameObject>();
    public Color SolidClearColor = Color.black;

    [Range(1, 60)]
    public float PlayerScale = 20;

    public Material HandParticleMaterial;

    public SentimentSpawnNode.Sentiment SentimentForRandomTweet = SentimentSpawnNode.Sentiment.Neutral;

    [Header("Auto")]
    public ParticleCityAnimator Animator;

    private bool destroyRequested = false;

    void Awake()
    {
        Animator = GetComponent<ParticleCityAnimator>();
    }

    void Update()
    {
        if (destroyRequested)
        {
            return;
        }

        InputManager.Instance.CenterCamera.clearFlags = CameraClearFlags.Color;
        InputManager.Instance.CenterCamera.backgroundColor = Color.Lerp(InputManager.Instance.CenterCamera.backgroundColor, SolidClearColor, 0.1f);
    }

    public void DestroyWithFadeOut()
    {
        destroyRequested = true;
        Animator.FadeOut(true);

        ParticleMotionBase[] motions = GetComponents<ParticleMotionBase>();
        for (int i = 0; i < motions.Length; i++)
        {
            Destroy(motions[i]);
        }
    }

    public void AddActiveGameObject(GameObject gameObject)
    {
        ActiveGameObjects.Add(gameObject);
    }

    public void RemoveActiveGameObject(GameObject gameObject, float? delay = null)
    {
        if (!delay.HasValue)
        {
            ActiveGameObjects.Remove(gameObject);
        }
        else
        {
            StartCoroutine(delayedRemove(gameObject, delay.Value));
        }
    }

    private IEnumerator delayedRemove(GameObject gameObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        ActiveGameObjects.Remove(gameObject);
    }
}
