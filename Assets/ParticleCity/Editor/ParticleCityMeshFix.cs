using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ParticleCityMeshFix {

    [MenuItem("ParticleCity/Fix Mesh")]
    public static void FixMesh()
    {
        foreach (GameObject gameObject in Selection.gameObjects)
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                continue;
            }

            Mesh mesh = meshFilter.sharedMesh;
            string path = string.Format("Assets/ParticleCityGen/{0}.asset", gameObject.name);
            AssetDatabase.CreateAsset(mesh, path);
        }
    }
}
