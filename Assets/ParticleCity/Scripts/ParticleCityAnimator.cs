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

    public float Size = 2;
    private float oldSize = -1;

    private float? targetIntensity;
    private float intensityLerpRatio;

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
    }

    void Update () 
    {
        if (targetIntensity.HasValue)
        {
            GlobalIntensity = Mathf.Lerp(GlobalIntensity, targetIntensity.Value, intensityLerpRatio);
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
	}

    public void LerpToIntensity(float targetIntensity, float ratio)
    {
        this.targetIntensity = targetIntensity;
        intensityLerpRatio = ratio;
    }

    public void SetMaterialsFloat(string propertyName, float value)
    {
        if (Application.isPlaying)
        {
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
}
