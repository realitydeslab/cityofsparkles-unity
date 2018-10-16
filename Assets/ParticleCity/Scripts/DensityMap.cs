using System;
using ParticleCities;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(BoxCollider))]
public class DensityMap : MonoBehaviour
{
    public static int DEBUG_POINT = 16384;

    public DensityMapData Data;
    public Texture3D Texture;

    [Header("Internal")]
    public Bounds Bounds;

    private Color32[] pixelsCache;

    // Debug
    [Header("Debug")]
    public bool DebugGizmo;
    public bool DebugMask;
    [Range(0, 256)]
    public float DebugDensityNorm = 256.0f;
    private Texture2D debugTexture;

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
        if (Data == null || Texture == null)
        {
            return 0;
        }

        if (pixelsCache == null)
        {
            pixelsCache = Texture.GetPixels32();
        }

        Vector3 offsetPos = (pos - Bounds.min);
        Vector3 densityPosNorm = new Vector3(
            offsetPos.x / Bounds.size.x,
            offsetPos.y / Bounds.size.y,
            offsetPos.z / Bounds.size.z
        );

        return trilinearInterp(densityPosNorm).r;
    }

    private Color trilinearInterp(Vector3 densityPosNorm)
    {
        // https://en.wikipedia.org/wiki/Trilinear_interpolation

        int x0, x1, y0, y1, z0, z1;
        float xd, yd, zd;
        coordClamp(densityPosNorm.x, Texture.width, out x0, out x1, out xd);
        coordClamp(densityPosNorm.y, Texture.height, out y0, out y1, out yd);
        coordClamp(densityPosNorm.z, Texture.depth, out z0, out z1, out zd);

        Color c000 = getPixel(pixelsCache, x0, y0, z0);
        Color c001 = getPixel(pixelsCache, x0, y0, z1);
        Color c010 = getPixel(pixelsCache, x0, y1, z0);
        Color c011 = getPixel(pixelsCache, x0, y1, z1);
        Color c100 = getPixel(pixelsCache, x1, y0, z0);
        Color c101 = getPixel(pixelsCache, x1, y0, z1);
        Color c110 = getPixel(pixelsCache, x1, y1, z0);
        Color c111 = getPixel(pixelsCache, x1, y1, z1);

        Color c00 = c000 * (1 - xd) + c100 * xd;
        Color c01 = c001 * (1 - xd) + c101 * xd;
        Color c10 = c010 * (1 - xd) + c110 * xd;
        Color c11 = c011 * (1 - xd) + c111 * xd;

        Color c0 = c00 * (1 - yd) + c10 * yd;
        Color c1 = c01 * (1 - yd) + c11 * yd;

        return c0 * (1 - zd) + c1 * zd;
    }

    private void coordClamp(float normalized, int size, out int v0, out int v1, out float d)
    {
        v0 = (int)(normalized * size);
        v0 = v0 < 0 ? 0 : (v0 >= size ? size - 1 : v0);
        v1 = (v0 >= size - 1) ? v0 : v0 + 1;
        d = (v0 == v1) ? 0 : (Mathf.Clamp01(normalized) * size - v0);
    }

    private Color getPixel(Color32[] pixels, int x, int y, int z)
    {
        int index = z * Texture.width * Texture.height + y * Texture.width + x;
        Color32 c = pixels[index];
        return new Color(
            c.r / 256.0f,
            c.g / 256.0f,
            c.b / 256.0f,
            c.a / 256.0f
        );
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
            Color32[] pixels = Texture.GetPixels32(0);

            int step = (int) Math.Pow(Texture.width * Texture.height * Texture.depth / (double)DEBUG_POINT, 1.0/3.0);

            for (int z = 0; z < Texture.depth; z += step)
            {
                for (int y = 0; y < Texture.height; y += step)
                {
                    for (int x = 0; x < Texture.width; x += step)
                    {
                        int index = z * (Texture.height * Texture.width) + 
                                    y * (Texture.width) +
                                    x;

                        Color32 pixel = pixels[index];
                        Vector3 pos = new Vector3(
                            Mathf.Lerp(b.min.x, b.max.x, (float)x / Texture.width),
                            Mathf.Lerp(b.min.y, b.max.y, (float)y / Texture.height),
                            Mathf.Lerp(b.min.z, b.max.z, (float)z / Texture.depth)
                        );

                        if (pixel.r > 0)
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
