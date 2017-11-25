using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class ParticleMotion : ParticleMotionBase {

    public Transform RightHand;
    public Transform LeftHand;

    protected override void UpdateInput() 
    { 
        // Update input
        particleMotionBlitMaterial.SetVector("_RightHandPos", new Vector4(RightHand.position.x, RightHand.position.y, RightHand.position.z, 1));
        particleMotionBlitMaterial.SetVector("_LeftHandPos", new Vector4(LeftHand.position.x, LeftHand.position.y, LeftHand.position.z, 1));
    }
}
