using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCityPlayerController : MonoBehaviour
{
    public OVRCameraRig CameraRig;
    public Transform PlayerStartingPoint;
    public bool FlyMode;
    public float FlyFullSpeed = 100; 

    private OVRPlayerController ovrPlayerController;
    private CharacterController characterController;
    private Transform playerTransform;

    private ParticleSystem leftParticle;
    private ParticleSystem rightParticle;
    private float particleFullRate;

	void Start ()
	{
        // TODO: Cross platform
	    playerTransform = CameraRig.transform.parent;
	    ovrPlayerController = playerTransform.GetComponent<OVRPlayerController>();
	    characterController = playerTransform.GetComponent<CharacterController>();

	    leftParticle = CameraRig.leftHandAnchor.GetComponentInChildren<ParticleSystem>(true);
	    rightParticle = CameraRig.rightHandAnchor.GetComponentInChildren<ParticleSystem>(true);
	    particleFullRate = leftParticle.emission.rateOverTimeMultiplier;

	    if (PlayerStartingPoint != null)
	    {
	        playerTransform.position = PlayerStartingPoint.position;
	        playerTransform.rotation = PlayerStartingPoint.rotation;
	        playerTransform.localScale = PlayerStartingPoint.localScale;
	    }

	}
	
	void Update () 
    {
        ovrPlayerController.enabled = !FlyMode;
        characterController.enabled = !FlyMode;

        leftParticle.gameObject.SetActive(FlyMode);
        rightParticle.gameObject.SetActive(FlyMode);

        if (FlyMode)
        {
            updateFlyMode();
        }		
	}

    private void updateFlyMode()
    {
        float leftTrigger = OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger);
        float rightTrigger = OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger);

        Transform activeHand = null;
        float activeTrigger = 0;

        if (leftTrigger > rightTrigger && leftTrigger > 0.01f)
        {
            activeHand = CameraRig.leftHandAnchor;
            activeTrigger = leftTrigger;
            rightTrigger = 0;
        }
        else if (rightTrigger > leftTrigger && rightTrigger > 0.01f)
        {
            activeHand = CameraRig.rightHandAnchor;
            activeTrigger = rightTrigger;
            leftTrigger = 0;
        }
        else
        {
            leftTrigger = 0;
            rightTrigger = 0;
        }

        if (activeHand != null)
        { 
            Vector3 movement = activeHand.forward * FlyFullSpeed * activeTrigger * Time.deltaTime;
            playerTransform.transform.position += movement;
        }

        ParticleSystem.EmissionModule leftEmission = leftParticle.emission;
        leftEmission.rateOverTimeMultiplier = particleFullRate * leftTrigger;

        ParticleSystem.EmissionModule rightEmission = rightParticle.emission;
        rightEmission.rateOverTimeMultiplier = particleFullRate * rightTrigger;

    }
}
