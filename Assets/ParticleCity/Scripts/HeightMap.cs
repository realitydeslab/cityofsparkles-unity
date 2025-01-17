﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

namespace ParticleCities
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(BoxCollider))]
    public class HeightMap : MonoBehaviour
    {
        public static int DEBUG_POINT = 16384;

        public Texture2D HeightMapTex;
        public bool DebugGizmo;

        [Header("Internal")] 
        public Bounds Bounds;

        void Awake()
        {
            Bounds = GetComponent<BoxCollider>().bounds;
        }

        void Start()
        {
        }

        void Update()
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        public bool GetHeightRange(Vector3 worldPosition, out float bottom, out float top)
        {
            bottom = 0;
            top = 0;

            float u = (worldPosition.x - Bounds.min.x) / Bounds.size.x;
            float v = (worldPosition.z - Bounds.min.z) / Bounds.size.z;

            if (u < 0 || u > 1 || v < 0 || v > 1)
            {
                return false;
            }

            Profiler.BeginSample("GetPixel");
            Color c = HeightMapTex.GetPixel((int) (u * HeightMapTex.width + 0.5f), (int) (v * HeightMapTex.height + 0.5f));
            Profiler.EndSample();

            if ((int)c.b >= 0)
            {
                bottom = Mathf.Lerp(Bounds.min.y, Bounds.max.y, c.r);
                top = Mathf.Lerp(Bounds.min.y, Bounds.max.y, c.g);
                return true;
            }

            return false;
        }

        public Vector3 PointWithRandomHeight(float x, float z)
        {
            float top, bottom, y;
            if (GetHeightRange(new Vector3(x, 0, z), out bottom, out top))
            {
                y = Random.Range(bottom, top);
            }
            else
            {
                y = Random.Range(Bounds.min.y, Bounds.max.y);
            }

            return new Vector3(x, y, z);
        }

        void OnDrawGizmosSelected()
        {
            if (DebugGizmo)
            {
                drawDebugGizmo();
            }
        }

        [Conditional("UNITY_EDITOR")]
        private void drawDebugGizmo()
        {
            Bounds b = GetComponent<BoxCollider>().bounds;
            if (DebugGizmo)
            {
                int limit = (int)Math.Sqrt(DEBUG_POINT);
                int xStep = Math.Max(1, HeightMapTex.width / limit);
                int yStep = Math.Max(1, HeightMapTex.height / limit);

                for (int y = 0; y < HeightMapTex.height; y += yStep)
                {
                    float v = (float) y / HeightMapTex.height;
                    for (int x = 0; x < HeightMapTex.width; x += xStep)
                    {
                        float u = (float) x / HeightMapTex.width;
                        float posX = Mathf.Lerp(b.min.x, b.max.x, u);
                        float posZ = Mathf.Lerp(b.min.z, b.max.z, v);

                        Color px = HeightMapTex.GetPixel(x, y);
                        int layer = (int) px.b;
                        float bottom = Mathf.Lerp(b.min.y, b.max.y, px.r);
                        float top = Mathf.Lerp(b.min.y, b.max.y, px.g);

                        if (layer >= 0)
                        {
                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(new Vector3(posX, bottom, posZ), new Vector3(posX, top, posZ));
                        }
                    }
                }
            }            
        }
    }
}
