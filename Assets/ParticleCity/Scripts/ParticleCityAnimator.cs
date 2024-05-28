using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParticleCityAnimator : MonoBehaviour
{
    // TODO: 1. Should not look for renderers like this
    // TODO: 2. Why bother creating instances? 

    private Material[] materialInstances;

    public float GlobalIntensity = 1;
    private float oldGlobalIntensity = -1;

    public float IntensityFadeOnStart = -1;
    public float IntensityLerpRatioOnStart = -1;
    public float IntensityLerpRatioOnFadeOut = 0.01f;

    public float Size = 2;
    private float oldSize = -1;

    public Vector4 NoiseST = new Vector4(1, 1, 0, 0);
    private Vector4 oldNoiseST = new Vector4(1, 1, 0, 0);

    private float? targetIntensity;
    private float intensityLerpRatio;

    private bool destroyRequired = false;

    private bool lerpToIntensityCalledOnce = false;

    // Yuchen added
    public bool m_IsFadingOut = false;

    void Awake()
    {
        if (Application.isPlaying)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            materialInstances = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                materialInstances[i] = renderers[i].material;
            }
        }
    }

	void Start () 
	{
        SetMaterialsFloat("_GlobalIntensity", GlobalIntensity);

        // !lerpToIntensityCalledOnce: Do not override if lerpToIntensity already set
	    if (Application.isPlaying && IntensityFadeOnStart >= 0 && !lerpToIntensityCalledOnce)
	    {
            LerpToIntensity(IntensityFadeOnStart, IntensityLerpRatioOnStart);    
	    }
    }

    void Update () 
    {
        if (targetIntensity.HasValue)
        {
            GlobalIntensity = Mathf.Lerp(GlobalIntensity, targetIntensity.Value, intensityLerpRatio / 0.016f * Time.deltaTime);
        }

        if (!Mathf.Approximately(GlobalIntensity, oldGlobalIntensity))
        {
            SetMaterialsFloat("_GlobalIntensity", GlobalIntensity);
            oldGlobalIntensity = GlobalIntensity;
        }

        if (!Mathf.Approximately(Size, oldSize))
        {
            SetMaterialsFloat("_Size", Size);
            oldSize = Size;
        }

        if (NoiseST != oldNoiseST) 
        {
            SetMaterialsFloat4("_NoiseTex_ST", NoiseST);
            oldNoiseST = NoiseST;
        }

        if (destroyRequired && GlobalIntensity < 0.01f)
        {
            Destroy(gameObject);
        }

	}

    public void LerpToIntensity(float targetIntensity, float ratio)
    {
        if (m_IsFadingOut)
            return;

        //Debug.Log($"[ParticleCityAnimator] gameObject.name: {gameObject.name}, targetIntensity: {targetIntensity}, ratio: {ratio}");
        // Yuchen added
        //if (targetIntensity == 0.1f)
        //    return;

        lerpToIntensityCalledOnce = true;
        this.targetIntensity = targetIntensity;
        intensityLerpRatio = ratio;
    }

    public void FadeOut(bool destroyOnFinished)
    {
        LerpToIntensity(0, IntensityLerpRatioOnFadeOut);
        m_IsFadingOut = true;
        if (destroyOnFinished)
        {
            destroyRequired = true;
        }
    }

    public void SetMaterialsFloat(string propertyName, float value)
    {
        if (Application.isPlaying)
        {
            // Yuchen added
            if (materialInstances.Length == 0)
            {
                if (TryGetComponent<ParticleCityGPUInstancingRenderer>(out var particleCityRenderer))
                {
                    var material = new Material(particleCityRenderer.Material);
                    particleCityRenderer.Material = material;
                    materialInstances = new Material[] { material };
                }
            }
            
            for (int i = 0; i < materialInstances.Length; i++)
            {
                materialInstances[i].SetFloat(propertyName, value);
            }
        }
        else
        {
            Renderer renderer = GetComponentInChildren<Renderer>();
            renderer.sharedMaterial.SetFloat(propertyName, value);
        }
    }

    public void SetMaterialsFloat4(string propertyName, Vector4 value)
    {
        if (Application.isPlaying)
        {
            for (int i = 0; i < materialInstances.Length; i++)
            {
                materialInstances[i].SetVector(propertyName, value);
            }
        }
        else
        {
            Renderer renderer = GetComponentInChildren<Renderer>();
            renderer.sharedMaterial.SetVector(propertyName, value);
        }
    }
}
