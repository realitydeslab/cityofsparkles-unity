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
    public ParticleCityGenMeshFormat MeshFormat;

    public int InstanceCount;
    public int RowsPerInstance;
}

public enum ParticleCityGenSampleMethod
{
    Surface,
    Volume,
}

public enum ParticleCityGenMeshFormat
{
    WithGeometryShader,
    NoGeometryShader,
    GPUInstancing
}

public enum ParticleCityGenTextureSize
{
    TextureSize1024 = 1024,
    TextureSize2048 = 2048,
    TextureSize4096 = 4096
}
