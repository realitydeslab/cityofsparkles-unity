using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Random = UnityEngine.Random;

public class ParticleCityGen : EditorWindow {

    private float samplePerCubeUnit = 0.0005f;
    private bool shouldGenDebugParticles = false;
    private bool shouldGenTextures = true;
    private bool shouldGenMesh = true;

    private const int TEX_WIDTH = 1024;
    private const int TEX_HEIGHT = 1024;
    private const int MAX_MESH_VERTEX = 65000;

    private List<Vector3> points = new List<Vector3>(1024 * 1024);
    private GameObject debugParticles = null;
    private Texture2D positionTexture = null;
    private GameObject particleCity = null;

    [MenuItem("ParticleCity/Particle City Gen")]
    static void ShowWindow() {
        EditorWindow.GetWindow<ParticleCityGen>();
    }

    void OnGUI() {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.Space();

        samplePerCubeUnit = EditorGUILayout.FloatField("Sample Per Cube Unit", samplePerCubeUnit);

        shouldGenDebugParticles = EditorGUILayout.Toggle("Debug Particle", shouldGenDebugParticles);
        shouldGenTextures = EditorGUILayout.Toggle("Build Textures", shouldGenTextures);
        shouldGenMesh = EditorGUILayout.Toggle("Generate Mesh", shouldGenMesh);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate")) {
            generateParticles();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Clear")) {
            clearParticles();
        }

        EditorGUILayout.EndVertical();
    }

    private void generateParticles() {
        var colliders = Selection.GetFiltered(typeof(Collider), SelectionMode.Deep).Select((obj) => (Collider)obj).ToArray();

        if (colliders.Length == 0) {
            Debug.LogError("Particle City Gen: No model selected");
            return;
        }

        loadGeneratedAssets();

        samplePoints(colliders);

        if (shouldGenDebugParticles) {
            genDebugParticles();
        }

        if (shouldGenTextures) {
            genPositionTexture();
        }

        if (shouldGenMesh) {
            genMesh();
        }

        AssetDatabase.SaveAssets();
    }

    private void samplePoints(Collider[] colliders) {
        Debug.Log("Particle City Gen: Samping particles");
        Debug.Log("Found " + colliders.Length + " colliders");

        points.Clear();

        int totalSampleCount = 0;

        bool cancel = false;

        for (int i = 0; i < colliders.Length; i++) {
            if (cancel) {
                break;
            }

            Collider collider = colliders[i];

            Vector3 boundsMaxLocal = collider.transform.InverseTransformPoint(collider.bounds.max);
            Vector3 boundsMinLocal = collider.transform.InverseTransformPoint(collider.bounds.min);
            Vector3 boundsDelta = boundsMaxLocal - boundsMinLocal;

            float volume = Mathf.Abs(boundsDelta.x * boundsDelta.y * boundsDelta.z);
            int sampleCount = (int)(volume * samplePerCubeUnit);

            totalSampleCount += sampleCount;
            Debug.Log("Sample count: " + sampleCount + ", Total: " + totalSampleCount);

            for (int j = 0; j < sampleCount; j++) {
                Vector3? p = sample(collider);
                if (p.HasValue) {
                    points.Add(p.Value);
                } else {
                    break;
                }
            }

            cancel = EditorUtility.DisplayCancelableProgressBar("Samping Particles", "Sampled Count " + totalSampleCount, (float)i / colliders.Length);
        }

        Debug.Log("Total sample count: " + totalSampleCount);
        Debug.Log(points.Count + " points sampled");

        if (points.Count > TEX_WIDTH * TEX_HEIGHT) {
            Debug.LogError("Particle City Gen: Too many points for specified texture size");
        }

        EditorUtility.ClearProgressBar();
    }

    private void genDebugParticles() {
        // TODO: null check
        var debugParticlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ParticleCity/DebugParticle.prefab");

        debugParticles = new GameObject();

        Debug.Log("Generating debug particles...");
        for (int i = 0; i < points.Count; i++) {
            GameObject debugParticle = Instantiate(debugParticlePrefab);
            debugParticle.transform.position = points[i];
            debugParticle.transform.parent = debugParticles.transform;
        }
        Debug.Log("Debug particles generated.");
    }

    private void genPositionTexture() {
        if (TEX_WIDTH * TEX_HEIGHT < points.Count) {
            Debug.LogError("Texture is too small to hold " + points.Count + " points.");
        }

        Debug.Log("Building position texture...");

        // TODO Perf: Generate raw data
        Color[] colors = new Color[TEX_WIDTH * TEX_HEIGHT];
        for (var i = 0; i < points.Count; i++) {
            colors[i] = new Color(points[i].x, points[i].y, points[i].z);
        }

        positionTexture = new Texture2D(TEX_WIDTH, TEX_HEIGHT, TextureFormat.RGBAFloat, false, true);
        positionTexture.anisoLevel = 1;
        positionTexture.filterMode = FilterMode.Point;
        positionTexture.SetPixels(colors);
        positionTexture.Apply();

        AssetDatabase.CreateAsset(positionTexture, "Assets/ParticleCityGen/ParticlePositions.asset");

        // Update material
        var particleCityGenMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/ParticleCity/ParticleCity.mat");
        particleCityGenMat.SetTexture("_PositionTex", positionTexture);

        var particleMotionBlitMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/ParticleCity/ParticleMotionBlit.mat");
        particleMotionBlitMat.SetTexture("_BasePositionTex", positionTexture);

        Debug.Log("Positions texture saved to Assets/ParticleCityGen/ParticlePositions.asset");
    }

    private void genMesh() {

        int pointCount = points.Count;

        // https://github.com/keijiro/KvantStream/blob/master/Assets/Kvant/Stream/Stream.cs CreateMesh

        int Nx = TEX_WIDTH;
        int Ny = ceiling(pointCount, TEX_WIDTH); // int ceiling

        Debug.Log("Creating grid mesh " + Nx + "x" + Ny + "...");

        // Create vertex arrays.
        var vertexArray = new Vector3[Nx * Ny];
        var uvArray = new Vector2[Nx * Ny];

        var index = 0;
        for (var x = 0; x < Nx; x++) {
            for (var y = 0; y < Ny; y++) {
                vertexArray[index] = new Vector3(x, 0, y);

                var u = (float)x / TEX_WIDTH;
                var v = (float)y / TEX_HEIGHT;
                uvArray[index] = new Vector2(u, v);

                index += 1;
            }
        }

        // Index array.
        var indexArray = new int[vertexArray.Length];
        for (index = 0; index < vertexArray.Length; index++) indexArray[index] = index;

        // Create a mesh object.
        int meshCount = ceiling(pointCount, MAX_MESH_VERTEX);
        var meshes = new Mesh[meshCount];

        for (var i = 0; i < meshCount; i++) {
            int start = i * MAX_MESH_VERTEX;

            meshes[i] = new Mesh{
                vertices = vertexArray.Skip(start).Take(MAX_MESH_VERTEX).ToArray(),
                uv = uvArray.Skip(start).Take(MAX_MESH_VERTEX).ToArray()
            };

            meshes[i].SetIndices(indexArray.Take(meshes[i].vertexCount).ToArray(), MeshTopology.Points, 0);
			MeshUtility.Optimize(meshes[i]);

            // Avoid being culled.
            meshes[i].bounds = new Bounds(Vector3.zero, Vector3.one * 10000);

            // TODO: Not tested!!!
            AssetDatabase.CreateAsset(meshes[i], string.Format("Assets/ParticleCity/Mesh {0}.asset", i));
        }

        // Create Prefab
        particleCity = new GameObject("Particle City", typeof(ParticleMotion));
        var particleMotion = particleCity.GetComponent<ParticleMotion>();
        particleMotion.BasePositionTexture = positionTexture;
        particleMotion.ParticleMotionBlitMaterialPrefab = AssetDatabase.LoadAssetAtPath<Material>("Assets/ParticleCity/ParticleMotionBlit.mat");
        particleMotion.LeftHand = GameObject.Find("/[CameraRig]/Controller (left)").transform;
        particleMotion.RightHand = GameObject.Find("/[CameraRig]/Controller (right)").transform;

        for (int i = 0; i < meshes.Length; i++) {
            GameObject meshObject = new GameObject("Mesh " + i, typeof(MeshFilter), typeof(MeshRenderer));
            meshObject.transform.parent = particleCity.transform;

            meshObject.GetComponent<MeshFilter>().mesh = meshes[i];
            meshObject.GetComponent<MeshRenderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/ParticleCity/ParticleCity.mat");
        }

        PrefabUtility.CreatePrefab("Assets/ParticleCityGen/ParticleCityPrefab.prefab", particleCity);

        Debug.Log("Prefab saved to Assets/ParticleCityGen/ParticleCityPrefab.prefab");
    }

    private Vector3? sample(Collider collider) {
        // TODO: Better sampling

        var bounds = collider.bounds;

        for (int retry = 0; retry < 100; retry++) {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            float z = Random.Range(bounds.min.z, bounds.max.z);
            var p = new Vector3(x, y, z);
            if (isPointInsideCollider(p, collider)) {
                return p;
            }
        }

        return null;
    }

    private void clearParticles() {
        if (debugParticles != null) {
            while (debugParticles.transform.childCount > 0) {
                DestroyImmediate(debugParticles.transform.GetChild(0).gameObject);
            }

            DestroyImmediate(debugParticles);
            debugParticles = null;
        }

        if (particleCity != null) {
            DestroyImmediate(particleCity);
            particleCity = null;
        }
        positionTexture = null;

        AssetDatabase.DeleteAsset("Assets/ParticleCityGen/ParticlePositions.asset");
        AssetDatabase.DeleteAsset("Assets/ParticleCityGen/ParticleCityPrefab.prefab");
    }

    private bool isPointInsideCollider(Vector3 point, Collider collider) {
        // TODO
        var start = new Vector3(0, 10000, 0); // This is defined to be some arbitrary point far away from the collider.

        Vector3 goal = point; // This is the point we want to determine whether or not is inside or outside the collider.
        Vector3 direction = goal - start; // This is the direction from start to goal.
        direction.Normalize();
        var iterations = 0; // If we know how many times the raycast has hit faces on its way to the target and back, we can tell through logic whether or not it is inside.
        Vector3 currentPoint = start;

        int retryCount = 0;
        while (currentPoint != goal && retryCount < 100) {// Try to reach the point starting from the far off point.  This will pass through faces to reach its objective.
            retryCount++;
            RaycastHit hit;
            if (Physics.Linecast(currentPoint, goal, out hit)) {// Progressively move the point forward, stopping everytime we see a new plane in the way.
                iterations++;
                currentPoint = hit.point + (direction / 100.0f); // Move the Point to hit.point and push it forward just a touch to move it through the skin of the mesh (if you don't push it, it will read that same point indefinately).
            } else {
                currentPoint = goal; // If there is no obstruction to our goal, then we can reach it in one step.
            }
        }

        retryCount = 0;
        while (currentPoint != start && retryCount < 100) {// Try to return to where we came from, this will make sure we see all the back faces too.
            retryCount++;
            RaycastHit hit;
            if (Physics.Linecast(currentPoint, start, out hit)) {
                iterations++;
                currentPoint = hit.point + (-direction / 100.0f);
            } else {
                currentPoint = start;
            }
        }

        return (iterations % 2 == 1);
    }

    private void loadGeneratedAssets() {
        // Try to load generated assets
        positionTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ParticleCityGen/ParticlePositions.asset");
        particleCity = GameObject.Find("/Particle City");
        debugParticles = GameObject.Find("/DebugParticles");
    }

    private static int ceiling(int a, int b) {
        return 1 + (int) ((a - 1) / b);
    }
}
