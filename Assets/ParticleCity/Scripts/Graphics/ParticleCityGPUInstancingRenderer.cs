using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class ParticleCityGPUInstancingRenderer : MonoBehaviour
{
    public Mesh Mesh;
    public Texture2D PositionTexture;
    public Material Material;
    public ParticleCityGenParams GenParams;

    private List<Matrix4x4> matrices;
    private MaterialPropertyBlock materialPropertyBlock;

    void OnEnable() {
        matrices = new List<Matrix4x4>(GenParams.InstanceCount);
        for (int i = 0; i < GenParams.InstanceCount; i++)
        {
            matrices.Add(transform.localToWorldMatrix);
        }

        float[] offsets = new float[GenParams.InstanceCount];
        for (int i = 0; i < GenParams.InstanceCount; i++)
        {
            offsets[i] = (float)(GenParams.RowsPerInstance * i) / PositionTexture.height;
        }

        materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetFloatArray("_InstancingRowOffset", offsets);

        Camera.onPreCull -= drawWithCamera;
        Camera.onPreCull += drawWithCamera;
    }
 
    void OnDisable() {
        Camera.onPreCull -= drawWithCamera;
    }

    private void drawWithCamera(Camera camera)
    { 
        if (Mesh == null || PositionTexture == null || GenParams == null)
        {
            return;
        }

        if (transform.localToWorldMatrix != matrices[0])
        {
            for (int i = 0; i < GenParams.InstanceCount; i++)
            {
                matrices[i] = transform.localToWorldMatrix;
            }
        }

        Graphics.DrawMeshInstanced(
            mesh: Mesh,
            submeshIndex: 0,
            material: Material,
            matrices: matrices,
            properties: materialPropertyBlock,
            castShadows: ShadowCastingMode.Off,
            receiveShadows: false,
            layer: 0,
            camera: camera,
            lightProbeUsage: LightProbeUsage.Off
            );

    }
}
