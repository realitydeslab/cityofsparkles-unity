using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCityGenParams : ScriptableObject
{
    public string GroupName = "untitled";

    public float SamplePerCubeUnit = 0.0005f;

    public float SamplePerSquareUnit = 100;
    public float TriangleEdgeSamplePerUnit = 10;

    public int TextureWidth = 2048;
    public int TextureHeight = 2048;

    public ParticleCityGenSampleMethod SampleMethod;
}

public enum ParticleCityGenSampleMethod
{
    Surface,
    Volume,
}
