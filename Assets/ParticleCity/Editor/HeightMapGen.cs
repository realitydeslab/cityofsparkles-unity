using System.Collections;
using System.Collections.Generic;
using ParticleCities;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditorInternal;

namespace ParticleCity.Editor
{
    public class HeightMapGen : EditorWindow
    {
        private HeightMap heightMapObj;
        private LayerMask layerMask;
        private string targetFolder = "ParticleCityGen";
        private int textureWidth = 4096;
        private int textureHeight = 4096;

        [MenuItem("ParticleCity/Height Map Gen")]
        static void ShowWindow()
        {
            EditorWindow.GetWindow<HeightMapGen>("HeightMapGen");
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            heightMapObj = (HeightMap) EditorGUILayout.ObjectField("Projector", heightMapObj, typeof(HeightMap), true);

            int tempMask = EditorGUILayout.MaskField( "Layer Mask", InternalEditorUtility.LayerMaskToConcatenatedLayersMask(layerMask), InternalEditorUtility.layers);
            layerMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);

            targetFolder = EditorGUILayout.TextField("Target Folder", targetFolder);
            textureWidth = EditorGUILayout.IntField("Texture Width", textureWidth);
            textureHeight = EditorGUILayout.IntField("Texture Height", textureHeight);

            if (GUILayout.Button("Generate"))
            {
                generateHeightMap();
            }

            EditorGUILayout.EndVertical();
        }

        private void generateHeightMap()
        {
            string target = Path.Combine("Assets", targetFolder);
            if (!Directory.Exists(target))
            {
                Directory.CreateDirectory(target);
            }

            // TODO: Maybe compress
            Texture2D tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBAFloat, false, true);
            tex.anisoLevel = 1;
            tex.filterMode = FilterMode.Point;

            for (int y = 0; y < textureHeight; y++)
            {
                float v = (float) y / textureHeight;
                for (int x = 0; x < textureWidth; x++)
                {
                    float u = (float) x / textureWidth;

                    float bottom, top;
                    int topLayer;
                    raycast(u, v, out bottom, out top, out topLayer);
                    tex.SetPixel(x, y, new Color(bottom, top, topLayer + 0.5f, 1));

                    // float d = topLayer > 0 ? 1 : 0;
                    // tex.SetPixel(x, y, new Color(d, d, d, 1));
                }

                bool cancel = EditorUtility.DisplayCancelableProgressBar("Raycasting", "Raycasting...", (float)y / textureHeight);
                if (cancel)
                {   
                    EditorUtility.ClearProgressBar();
                    return;
                }
            }
            tex.Apply();

            HeightMapData data = ScriptableObject.CreateInstance<HeightMapData>();
            data.HeightMapTexture = tex;
            EditorUtility.SetDirty(data);

            AssetDatabase.CreateAsset(data, Path.Combine(target, "HeightMapData.asset"));
            AssetDatabase.AddObjectToAsset(tex, data);
            AssetDatabase.SaveAssets();

            heightMapObj.Data = data;

            EditorUtility.ClearProgressBar();
        }

        private void raycast(float u, float v, out float bottom, out float top, out int topLayer)
        {
            bottom = 0;
            top = 0;
            topLayer = -10;
            Bounds b = heightMapObj.GetComponent<BoxCollider>().bounds;

            float x = Mathf.Lerp(b.min.x, b.max.x, u);
            float z = Mathf.Lerp(b.min.z, b.max.z, v);
            float distance = b.max.y - b.min.y; 

            // Bottom up
            Vector3 bottomOrigin = new Vector3(x, b.min.y, z);
            RaycastHit bottomHitInfo;
            if (Physics.Raycast(bottomOrigin, Vector3.up, out bottomHitInfo, distance, layerMask))
            {
                bottom = (bottomHitInfo.point.y - b.min.y) / (b.max.y - b.min.y);
            }

            // Top down
            Vector3 topOrigin = new Vector3(x, b.max.y, z);
            RaycastHit topHitInfo;
            if (Physics.Raycast(topOrigin, Vector3.down, out topHitInfo, distance, layerMask))
            {
                top = (topHitInfo.point.y - b.min.y) / (b.max.y - b.min.y);
                topLayer = topHitInfo.collider.gameObject.layer;
            }
        }
    }
}
