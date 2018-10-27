using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using WanderUtils;

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
        public float IdleTimeToReset = 10;
        public float ForceInitialSceneSwitchTime = 5;
        public float ForceSceneSwitchTime = 10;

        private static StageSwitcher instance;
        private float idleTime;
        private float lastSwitchTime;

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
            if (InputManager.Instance.IsDeviceIdle())
            {
                idleTime += Time.deltaTime;
            }
            else
            {
                idleTime = 0;
            }

            if (idleTime > IdleTimeToReset)
            {
                Bootloader.SceneToLoad = "new_york_opening";
                SceneManager.LoadScene("bootloader");
                return;
            }

            if (CurrentStage == Stage.InitialDark || CurrentStage == Stage.Intro)
            {
                if (Time.time - lastSwitchTime > ForceInitialSceneSwitchTime)
                {
                    ManualSwitchToStage(1);
                }
            }
            else if (CurrentStage == Stage.First || CurrentStage == Stage.Twist || CurrentStage == Stage.Last)
            {
                if (Time.time - lastSwitchTime > ForceSceneSwitchTime)
                {
                    ManualSwitchToStage((int)CurrentStage - 1 + 1);
                }
            }
            

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
                        ManualSwitchToStage(i);
                    }
                }
            }
        }

        public void ManualSwitchToStage(int i)
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

        public void SwitchToStage(int index)
        {
            lastSwitchTime = Time.time;
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
