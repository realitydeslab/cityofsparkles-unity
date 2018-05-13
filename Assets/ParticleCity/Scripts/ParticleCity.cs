using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleCityAnimator))]
public class ParticleCity : MonoBehaviour
{
    public static ParticleCity Current
    {
        get { return StageSwitcher.Instance.CurrentParticleCity; }
    }

    public List<GameObject> ActiveGameObjects = new List<GameObject>();

    [Header("Auto")]
    public ParticleCityAnimator Animator;

    void Awake()
    {
        Animator = GetComponent<ParticleCityAnimator>();
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
