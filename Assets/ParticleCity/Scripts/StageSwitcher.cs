using System;
using System.Collections;
using System.Collections.Generic;
using ParticleCities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSwitcher : MonoBehaviour
{
    public Transform CameraRig;
    public ParticleCity[] ParticleCityPrefabs;

    void Start()
    {
        // Destroy all cities in editor and spawn prefabs
        cleanup();

        if (ParticleCityPrefabs.Length == 0)
        {
            Debug.LogError("No particle city prefab specified in stage switcher.");
            return;
        }

        instantiateParticleCity(ParticleCityPrefabs[0]);
    }

	void Update ()
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

    private void switchToStage(int index)
    {
        cleanup();
        instantiateParticleCity(ParticleCityPrefabs[index]);
    }

    private void instantiateParticleCity(ParticleCity prefab)
    {
        ParticleCity instance = Instantiate(prefab);
        ParticleMotionBase motion = instance.GetComponent<ParticleMotionBase>();
        if (motion != null)
        {
            motion.CameraRig = CameraRig;
        }
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
