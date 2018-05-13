using System.Collections;
using System.Collections.Generic;
using ParticleCities;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEditorInternal;
using UnityEngine.Timeline;

namespace ParticleCity.Editor
{
    public class DensityMapGen : EditorWindow
    {
        private DensityMap densityMapObj;
        private Texture2D positionMap;
        private string targetFolder = "ParticleCityGen";
        private int textureWidth = 512;
        private int textureHeight = 16;
        private int textureDepth = 1024;
        private int gaussianBlurRadius = 10;

        [MenuItem("ParticleCity/Density Map Gen")]
        static void ShowWindow()
        {
            EditorWindow.GetWindow<DensityMapGen>("DensityMapGen");
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            densityMapObj = (DensityMap) EditorGUILayout.ObjectField("Target", densityMapObj, typeof(DensityMap), true);
            positionMap = (Texture2D) EditorGUILayout.ObjectField("Positions", positionMap, typeof(Texture2D), false);

            targetFolder = EditorGUILayout.TextField("Target Folder", targetFolder);
            textureWidth = EditorGUILayout.IntField("Texture Width", textureWidth);
            textureHeight = EditorGUILayout.IntField("Texture Height", textureHeight);
            textureDepth = EditorGUILayout.IntField("Texture Depth", textureDepth);
            gaussianBlurRadius = EditorGUILayout.IntField("Gaussian Blur Radius", gaussianBlurRadius);

            if (GUILayout.Button("Generate"))
            {
                generateDensityMap();
            }

            EditorGUILayout.EndVertical();
        }

        private void generateDensityMap()
        {
            string target = Path.Combine("Assets", targetFolder);
            if (!Directory.Exists(target))
            {
                Directory.CreateDirectory(target);
            }

            Texture3D tex = new Texture3D(textureWidth, textureHeight, textureDepth, TextureFormat.RGBA32, false)
            {
                anisoLevel = 1,
                filterMode = FilterMode.Trilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Color[] positionPixels = positionMap.GetPixels(0);
            Color32[] densityPixels = new Color32[textureWidth * textureHeight * textureDepth];

            // for (int z = 0; z < textureDepth; z++)
            // {
            //     for (int y = 0; y < textureHeight; y++)
            //     {
            //         for (int x = 0; x < textureWidth; x++)
            //         {
            //             int index = z * (textureHeight * textureWidth) + 
            //                         y * textureWidth +
            //                         x;

            //             densityPixels[index].r = (float)z / textureWidth;
            //         }
            //     }
            // }

            // Temp to channel a
            for (int i = 0; i < positionPixels.Length; i++)
            {
                Vector3 pos = new Vector3(positionPixels[i].r, positionPixels[i].g, positionPixels[i].b);
                Vector3 offsetPos = (pos - densityMapObj.Bounds.min);
                Vector3 densityPos = new Vector3(
                    Mathf.Clamp01(offsetPos.x / densityMapObj.Bounds.size.x) * textureWidth,
                    Mathf.Clamp01(offsetPos.y / densityMapObj.Bounds.size.y) * textureHeight,
                    Mathf.Clamp01(offsetPos.z / densityMapObj.Bounds.size.z) * textureDepth
                );

                int index = (int)(densityPos.z + 0.5f) * (textureWidth * textureHeight) +
                            (int)(densityPos.y + 0.5f) * textureWidth +
                            (int)(densityPos.x + 0.5f);

                densityPixels[index].a += 32;

                if (i % 100 == 0)
                {
                    bool cancel = EditorUtility.DisplayCancelableProgressBar("Density Map", "Generating " + i + " / " + positionPixels.Length, (float) i / positionPixels.Length);
                    if (cancel)
                    {
                        EditorUtility.ClearProgressBar();
                        break;
                    }
                }
            }

            // Gaussian blur
            float sigma = gaussianBlurRadius / 3.0f;
            float[] kernel = new float[gaussianBlurRadius * 2 + 1];
            for (int x = -gaussianBlurRadius; x <= gaussianBlurRadius; x++)
            {
                kernel[x + gaussianBlurRadius] = 1 / (Mathf.Sqrt(2 * Mathf.PI) * sigma) * Mathf.Exp(- x * x / (2 * sigma * sigma));
            }

            // x axis, blur a to r
            for (int z = 0; z < textureDepth; z++)
            {
                for (int y = 0; y < textureHeight; y++)
                {
                    for (int x = 0; x < textureWidth; x++)
                    {
                        float c = 0;
                        for (int dx = x - gaussianBlurRadius; dx <= x + gaussianBlurRadius; dx++)
                        {
                            int dxClamp = (dx < 0) ? 0 : ((dx >= textureWidth) ? (textureWidth - 1) : dx);
                            int dxIndex = z * (textureWidth * textureHeight) + y * textureWidth + dxClamp;
                            c += densityPixels[dxIndex].a / 256.0f * kernel[dx - x + gaussianBlurRadius];
                        }

                        int index = z * (textureWidth * textureHeight) + y * textureWidth + x;
                        densityPixels[index].r = (byte)(c * 256.0f + 0.5f);
                    }
                }

                bool cancel = EditorUtility.DisplayCancelableProgressBar("Density Map", "Blurring X Axis", (float)z / textureDepth);
                if (cancel)
                {
                    EditorUtility.ClearProgressBar();
                    break;
                }
            }

            // y axis, blur r to a
            for (int z = 0; z < textureDepth; z++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    for (int y = 0; y < textureHeight; y++)
                    {
                        float c = 0;
                        for (int dy = y - gaussianBlurRadius; dy <= y + gaussianBlurRadius; dy++)
                        {
                            int dyClamp = (dy < 0) ? 0 : ((dy >= textureHeight) ? (textureHeight - 1) : dy);
                            int dyIndex = z * (textureWidth * textureHeight) + dyClamp * textureWidth + x;
                            c += densityPixels[dyIndex].r / 256.0f * kernel[dy - y + gaussianBlurRadius];
                        }

                        int index = z * (textureWidth * textureHeight) + y * textureWidth + x;
                        densityPixels[index].a = (byte)(c * 256.0f + 0.5f);
                    }
                }

                bool cancel = EditorUtility.DisplayCancelableProgressBar("Density Map", "Blurring Y Axis", (float)z / textureDepth);
                if (cancel)
                {
                    EditorUtility.ClearProgressBar();
                    break;
                }
            }

            // z axis, blur a to r
            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    for (int z = 0; z < textureDepth; z++)
                    {
                        float c = 0;
                        for (int dz = z - gaussianBlurRadius; dz <= z + gaussianBlurRadius; dz++)
                        {
                            int dzClamp = (dz < 0) ? 0 : ((dz >= textureDepth) ? (textureDepth - 1) : dz);
                            int dzIndex = dzClamp * (textureWidth * textureHeight) + y * textureWidth + x;
                            c += densityPixels[dzIndex].a / 256.0f * kernel[dz - z + gaussianBlurRadius];
                        }

                        int index = z * (textureWidth * textureHeight) + y * textureWidth + x;
                        densityPixels[index].r = (byte)(c * 256.0f + 0.5f);
                    }
                }

                bool cancel = EditorUtility.DisplayCancelableProgressBar("Density Map", "Blurring Z Axis", (float)y / textureHeight);
                if (cancel)
                {
                    EditorUtility.ClearProgressBar();
                    break;
                }
            }

            tex.SetPixels32(densityPixels);
            tex.Apply();

            DensityMapData data = ScriptableObject.CreateInstance<DensityMapData>();
            data.DensityMapTexture = tex;
            data.GaussianBlurRadius = gaussianBlurRadius;

            EditorUtility.SetDirty(data);

            AssetDatabase.CreateAsset(data, Path.Combine(target, "DensityMapData.asset"));
            AssetDatabase.AddObjectToAsset(tex, data);
            AssetDatabase.SaveAssets();

            densityMapObj.Data = data;

            EditorUtility.ClearProgressBar();
        }

    }
}
