using System;
using System.Collections;
using System.Collections.Generic;
using ParticleCities;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
using WanderUtils;

public class OpeningStart : MonoBehaviour
{
    public LayerMask CitySelectionLayerMask;
    public Animator Selected;
    public PostProcessingProfile PostProcessingProfile;

    void Start()
    {
        InputManager.Instance.CenterCamera.GetComponent<PostProcessingBehaviour>().profile = PostProcessingProfile;
    }

    void Update()
    {
        Transform cameraTransform = Camera.main.transform;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hitInfo;
        if (Physics.Raycast(
            ray: ray,
            maxDistance: float.MaxValue,
            hitInfo: out hitInfo,
            layerMask: CitySelectionLayerMask))
        {
            if (hitInfo.collider.gameObject != Selected)
            {
                if (Selected != null)
                {
                    Selected.SetBool("Selected", false);
                }

                Selected = hitInfo.collider.GetComponent<Animator>();
                Selected.SetBool("Selected", true);
                Bootloader.SceneToLoad = Selected.gameObject.name;
            }
            
        }
        else if (Selected != null)
        {
            Selected.SetBool("Selected", false);
            Selected = null;
        }
            
        if (
            Selected != null && (
            InputManager.Instance.GetTriggerValue(HandType.Left) > 0.5f ||
            InputManager.Instance.GetTriggerValue(HandType.Right) > 0.5f ||
            InputManager.Instance.GetButtonDown(Button.Confirm))
        )
        {
            SceneManager.LoadScene("bootloader");
        }
    }
}
