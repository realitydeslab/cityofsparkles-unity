using System.Collections;
using System.Collections.Generic;
using ParticleCities;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;

public class OpeningStart : MonoBehaviour
{
    public string SceneToLoad = "new_york_scene";
    public PostProcessingProfile PostProcessingProfile;

    void Start()
    {
        InputManager.Instance.CenterCamera.GetComponent<PostProcessingBehaviour>().profile = PostProcessingProfile;
    }

    void Update()
    {
        if (
            InputManager.Instance.GetTriggerValue(HandType.Left) > 0.5f ||
            InputManager.Instance.GetTriggerValue(HandType.Right) > 0.5f ||
            InputManager.Instance.GetButtonDown(Button.Confirm)
        )
        {
            Bootloader.SceneToLoad = SceneToLoad;
            SceneManager.LoadScene("bootloader");
        }
    }
}
