using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCityAnimator : MonoBehaviour
{

    private Material[] materialInstances;

    public float GlobalIntensity = 1;
    private float oldGlobalIntensity;

    void Awake()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        materialInstances = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            materialInstances[i] = renderers[i].material;
        }
    }

	void Start () {
		
	}
	
	void Update () 
    {
        if (!Mathf.Approximately(GlobalIntensity, oldGlobalIntensity))
        {
            SetMaterialsFloat("_GlobalIntensity", GlobalIntensity);
            oldGlobalIntensity = GlobalIntensity;
        }     
	}

    public void SetMaterialsFloat(string propertyName, float value)
    {
        for (int i = 0; i < materialInstances.Length; i++)
        {
            materialInstances[i].SetFloat(propertyName, value);
        }    
    }
}
