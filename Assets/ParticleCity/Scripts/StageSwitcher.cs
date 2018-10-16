using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ParticleCities
{
    public enum Stage
    {
        Invalid = 0,
        Intro,
        First,
        Twist,
        Last,
        FinalSpawn,
        InitialDark
    }

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

        public Stage InitialStage;
        public ParticleCity[] ParticleCityPrefabs;
        public bool KeyboardSwitch;
        public Transform[] StoryGroupOnManualSwitch;

        [Header("Auto")] 
        public ParticleCity CurrentParticleCity;
        public Stage CurrentStage;

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

                SwitchToStage(InitialStage);
            }
        }

        void Update()
        {
            if (KeyboardSwitch)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    Bootloader.SceneToLoad = "new_york_opening";
                    SceneManager.LoadScene("bootloader");
                    return;
                }

                int keyNum = Math.Min(9, ParticleCityPrefabs.Length - 1);
                for (int i = 0; i <= keyNum; i++)
                {
                    KeyCode key = (KeyCode) ((int) KeyCode.Alpha0 + i);
                    if (Input.GetKeyDown(key))
                    {
                        SwitchToStage(i);

                        if (i < StoryGroupOnManualSwitch.Length && !StoryGroupOnManualSwitch[i].Equals(null))
                        {
                            StoryGroupOnManualSwitch[i].gameObject.SetActive(true); 
                            for (int j = 0; j < StoryGroupOnManualSwitch[i].childCount; j++)
                            {
                                Transform child = StoryGroupOnManualSwitch[i].GetChild(j);
                                if (!child.Equals(null))
                                {
                                    child.gameObject.SetActive(true);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SwitchToStage(int index)
        {
            cleanup();
            instantiateParticleCity(ParticleCityPrefabs[index]);
            CurrentStage = (Stage)(index + 1);
            AkSoundEngine.SetState("Stage", CurrentStage.ToString());
        }

        public void SwitchToStage(Stage stage)
        {
            if (stage == Stage.Invalid)
            {
                return;
            }

            SwitchToStage((int)stage - 1);
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
                ParticleCity city = rootObjects[i].GetComponent<ParticleCity>();
                if (city != null)
                {
                    city.DestroyWithFadeOut();
                }
            }
        }
    }
}
