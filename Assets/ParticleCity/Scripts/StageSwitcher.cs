using System;
using System.Collections;
using System.Collections.Generic;
using ParticleCities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSwitcher : MonoBehaviour
{
    private static StageSwitcher instance;
    public static StageSwitcher Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<StageSwitcher>();
            }

            return instance;
        }
    }

    public ParticleCity[] ParticleCityPrefabs;
    public bool KeyboardSwitch;

    [Header("Auto")] 
    public ParticleCity CurrentParticleCity;

    void Start()
    {
        // Find the current enabled city
        ParticleCity[] cities = FindObjectsOfType<ParticleCity>();
        foreach (ParticleCity city in cities)
        {
            if (city.enabled)
            {
                CurrentParticleCity = city;
                break;
            }
        }

        if (CurrentParticleCity == null)
        {
            if (ParticleCityPrefabs.Length == 0)
            {
                Debug.LogError("No particle city prefab specified in stage switcher.");
                return;
            }

            instantiateParticleCity(ParticleCityPrefabs[0]);
        }
    }

	void Update ()
	{
	    if (KeyboardSwitch)
	    {
	        int keyNum = Math.Min(9, ParticleCityPrefabs.Length);
	        for (int i = 1; i <= keyNum; i++)
	        {
	            KeyCode key = (KeyCode) ((int) KeyCode.Alpha0 + i);
	            if (Input.GetKeyDown(key))
	            {
	                switchToStage(i - 1);
	            }
	        }
	    }
	}

    private void switchToStage(int index)
    {
        cleanup();
        instantiateParticleCity(ParticleCityPrefabs[index]);
    }

    private void instantiateParticleCity(ParticleCity prefab)
    {
        CurrentParticleCity = Instantiate(prefab);
    }

    private void cleanup()
    {
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            if (rootObjects[i].GetComponent<ParticleCity>() != null)
            {
                Destroy(rootObjects[i]);
            }
        }

    }
}
