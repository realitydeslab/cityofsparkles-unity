using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCity : MonoBehaviour
{
    private static ParticleCity instance;
    public static ParticleCity Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ParticleCity>();
            }

            return instance;
        }
    }

    public List<GameObject> ActiveGameObjects = new List<GameObject>();

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
