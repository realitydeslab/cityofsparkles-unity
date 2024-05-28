using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using WanderUtils;
using System.Collections.Generic;
using System.Collections;

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
        public Transform StoryRoot;

        [Header("Auto")] 
        public ParticleCity CurrentParticleCity;
        public Stage CurrentStage;

        private Dictionary<ParticleCity, GameObject> m_ParticleCityPool = new();

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

            // Prepare particle city pool
            var particleCity = Instantiate(ParticleCityPrefabs[0]);
            particleCity.gameObject.SetActive(false);
            m_ParticleCityPool.Add(ParticleCityPrefabs[0], particleCity.gameObject);
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

            TwitterManager.Instance.ClearAll();
            StoryNode[] activeStoryNodes = StoryRoot.GetComponentsInChildren<StoryNode>();
            for (int j = 0; j < activeStoryNodes.Length; j++)
            {
                Destroy(activeStoryNodes[j].gameObject);
            }

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
            Debug.Log($"[StageSwitcher] SwitchToStage {index}");
            lastSwitchTime = Time.time;
            cleanup();

            // Yuchen updated
            if (index ==5 )
                instantiateParticleCity(ParticleCityPrefabs[index]);
            else
                StartCoroutine(instantiateParticleCityWithDelay(ParticleCityPrefabs[index], 7f));

            CurrentStage = (Stage)(index + 1);
            // TODO: Wwise
            //AkSoundEngine.SetState("Stage", CurrentStage.ToString());
        }

        public void SwitchToStage(Stage stage)
        {
            Debug.Log($"[StageSwitcher] SwitchToStage {stage}");
            if (stage == Stage.Invalid)
            {
                return;
            }

            SwitchToStage((int)stage - 1);
        }

        // Yuchen added
        private IEnumerator instantiateParticleCityWithDelay(ParticleCity prefab, float delay)
        {
            yield return new WaitForSeconds(delay);
            instantiateParticleCity(prefab);
        }

        private void instantiateParticleCity(ParticleCity prefab)
        {
            // Yuchen updated
            if (m_ParticleCityPool.ContainsKey(prefab))
            {
                m_ParticleCityPool[prefab].SetActive(true);
                CurrentParticleCity = m_ParticleCityPool[prefab].GetComponent<ParticleCity>();
            }   
            else
                CurrentParticleCity = Instantiate(prefab);
        }

        private void cleanup()
        {
            GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                ParticleCity city = rootObjects[i].GetComponent<ParticleCity>();
                // Yuchen upadted
                if (city != null && city.gameObject.activeSelf)
                {
                    city.DestroyWithFadeOut();
                }
            }
        }
    }
}
