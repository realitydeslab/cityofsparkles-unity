using System;
using ParticleCities;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(BoxCollider))]
public class DensityMap : MonoBehaviour
{
    public static int DEBUG_POINT = 16384;

    public DensityMapData Data;

    [Header("Internal")]
    public Bounds Bounds;

    private Color[] pixelsCache;

    // Debug
    [Header("Debug")]
    public bool DebugGizmo;
    public bool DebugMask;
    [Range(0, 10)]
    public float DebugDensityNorm = 2.0f;
    private Texture2D debugTexture;

    void Awake()
    {
        pixelsCache = Data.DensityMapTexture.GetPixels();
    }

    void Start () 
    {
        Bounds = GetComponent<BoxCollider>().bounds;
	}
	
	void Update () 
	{
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    void OnGUI ()
    {
        if (DebugMask)
        {
            if (debugTexture == null)
            {
                debugTexture = new Texture2D(1, 1);
            }

            float density = GetDensity(InputManager.Instance.PlayerTransform.position);
            Color c = new Color(density / DebugDensityNorm, 0, 0);
            debugTexture.SetPixel(0, 0, c);
            debugTexture.Apply();
            GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 50, 100, 100), debugTexture);
            GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 2 + 50, 100, 100), density.ToString());
        }
    }

    public float GetDensity(Vector3 pos)
    {
        if (pixelsCache == null)
        {
            pixelsCache = Data.DensityMapTexture.GetPixels();
        }

        Vector3 offsetPos = (pos - Bounds.min);
        Vector3 densityPos = new Vector3(
            Mathf.Clamp01(offsetPos.x / Bounds.size.x) * Data.DensityMapTexture.width,
            Mathf.Clamp01(offsetPos.y / Bounds.size.y) * Data.DensityMapTexture.height,
            Mathf.Clamp01(offsetPos.z / Bounds.size.z) * Data.DensityMapTexture.depth
        );

        int index = (int)(densityPos.z + 0.5f) * (Data.DensityMapTexture.width * Data.DensityMapTexture.height) +
                    (int)(densityPos.y + 0.5f) * Data.DensityMapTexture.height +
                    (int)(densityPos.x + 0.5f);

        return pixelsCache[index].r;
    }


    void OnDrawGizmosSelected()
    {
        if (DebugGizmo)
        {
            drawDebugGizmo();
        }
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void drawDebugGizmo()
    {
        if (DebugGizmo && Data)
        {
            Bounds b = GetComponent<BoxCollider>().bounds;
            Color[] pixelsCache = Data.DensityMapTexture.GetPixels(0);

            int step = (int) Math.Pow(Data.DensityMapTexture.width * Data.DensityMapTexture.height * Data.DensityMapTexture.depth / DEBUG_POINT, 1.0/3.0);

            for (int z = 0; z < Data.DensityMapTexture.depth; z += step)
            {
                for (int y = 0; y < Data.DensityMapTexture.height; y += step)
                {
                    for (int x = 0; x < Data.DensityMapTexture.width; x += step)
                    {
                        int index = z * (Data.DensityMapTexture.height * Data.DensityMapTexture.width) + 
                                    y * (Data.DensityMapTexture.width) +
                                    x;

                        Color pixel = pixelsCache[index];
                        Vector3 pos = new Vector3(
                            Mathf.Lerp(b.min.x, b.max.x, (float)x / Data.DensityMapTexture.width),
                            Mathf.Lerp(b.min.y, b.max.y, (float)y / Data.DensityMapTexture.height),
                            Mathf.Lerp(b.min.z, b.max.z, (float)z / Data.DensityMapTexture.depth)
                        );

                        if (!Mathf.Approximately(pixel.r, 0))
                        {
                            Gizmos.color = new Color(pixel.r / DebugDensityNorm, 0, 0, 1);
                            Gizmos.DrawSphere(pos, 10);
                        }
                    }
                }
            }
        }
    }
}

public class DensityMapData : ScriptableObject
{
    public Texture3D DensityMapTexture;
    public int GaussianBlurRadius;
}
