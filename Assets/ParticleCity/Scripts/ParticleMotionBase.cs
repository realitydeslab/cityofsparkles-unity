using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ParticleMotionBase : MonoBehaviour {

    public Texture2D BasePositionTexture;
    public Material ParticleMotionBlitMaterialPrefab;

    protected Material particleMotionBlitMaterial;

    private static RenderTexture particleVelocityBuffer1;
    private static RenderTexture particleVelocityBuffer2;

    private static RenderTexture particleOffsetBuffer1;
    private static RenderTexture particleOffsetBuffer2;

	public virtual void Start ()
	{
	    particleMotionBlitMaterial = ParticleMotionBlitMaterialPrefab; 
        particleMotionBlitMaterial.SetTexture("_BasePositionTex", BasePositionTexture);

	    if (particleOffsetBuffer1 == null)
	    {
	        particleOffsetBuffer1 = createRenderTexture();
	    }

	    if (particleOffsetBuffer2 == null)
	    {
	        particleOffsetBuffer2 = createRenderTexture();
	    }

	    if (particleVelocityBuffer1 == null)
	    {
	        particleVelocityBuffer1 = createRenderTexture();
	    }

	    if (particleOffsetBuffer2 == null)
	    {
            particleVelocityBuffer2 = createRenderTexture();
        }

        Graphics.Blit(null, particleVelocityBuffer1, particleMotionBlitMaterial, 0);
        Graphics.Blit(null, particleVelocityBuffer2, particleMotionBlitMaterial, 0);
        Graphics.Blit(null, particleOffsetBuffer1, particleMotionBlitMaterial, 0);
        Graphics.Blit(null, particleOffsetBuffer2, particleMotionBlitMaterial, 0);
	}
	
	public virtual void Update () {
	    if (Time.time < 3)
	    {
	        return;
	    }

        UpdateInput();

        // Update velocity
        particleMotionBlitMaterial.SetTexture("_OffsetTex", particleOffsetBuffer1);
        Graphics.Blit(particleVelocityBuffer1, particleVelocityBuffer2, particleMotionBlitMaterial, 1);
        swapBuffer(ref particleVelocityBuffer1, ref particleVelocityBuffer2);

        // Update offset
        particleMotionBlitMaterial.SetTexture("_VelocityTex", particleVelocityBuffer1);
        Graphics.Blit(particleOffsetBuffer1, particleOffsetBuffer2, particleMotionBlitMaterial, 2);
        swapBuffer(ref particleOffsetBuffer1, ref particleOffsetBuffer2);

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++) {
           renderers[i].material.SetTexture("_OffsetTex", particleOffsetBuffer1);
        }
	}

    public virtual void OnEnable() { }
    public virtual void OnDisable() { }

    public virtual void OnDestroy() { }

    protected abstract void UpdateInput();

    private static void swapBuffer(ref RenderTexture buff1, ref RenderTexture buff2) {
        RenderTexture tmp = buff1;
        buff1 = buff2;
        buff2 = tmp;
    }

    private RenderTexture createRenderTexture()
    {
        RenderTexture tex = new RenderTexture(BasePositionTexture.width, BasePositionTexture.height, 0, RenderTextureFormat.ARGBFloat);
        tex.useMipMap = false;
        tex.autoGenerateMips = false;
        tex.filterMode = FilterMode.Point;
        tex.anisoLevel = 0;

        return tex;
    }
}
