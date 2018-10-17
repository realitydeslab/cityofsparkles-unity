using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

public class ParticleCityGen : EditorWindow
{
    private ParticleCityGenParams genParams; 

    private Material cityMatTempalte;
    private Material motionBlitMatTemplate;

    private bool shouldGenDebugParticles = false;
    private bool shouldGenTextures = true;
    private bool shouldGenMesh = true;

    private const int MAX_MESH_VERTEX = 65000;

    private List<Vector3> points;
    private GameObject debugParticles = null;
    private Texture2D positionTexture = null;
    private GameObject particleCity = null;

    private ParticleCityGenParams genParamsToLoad;

    [MenuItem("ParticleCity/Particle City Gen")]
    static void ShowWindow() {
        EditorWindow.GetWindow<ParticleCityGen>();
    }

    void OnGUI() {
        // Default 
        if (genParams == null)
        {
            genParams = new ParticleCityGenParams();
        }

        if (cityMatTempalte == null)
        {
            cityMatTempalte = AssetDatabase.LoadAssetAtPath<Material>("Assets/ParticleCity/Materials/ParticleCityTemplate.mat");
        }

        if (motionBlitMatTemplate == null)
        {
            motionBlitMatTemplate = AssetDatabase.LoadAssetAtPath<Material>("Assets/ParticleCity/Materials/ParticleCityMotionTemplate.mat");
        }


        EditorGUILayout.BeginVertical();

        EditorGUILayout.Space();

        genParams.GroupName = EditorGUILayout.TextField("Name", genParams.GroupName);
        cityMatTempalte = (Material)EditorGUILayout.ObjectField("City Material", cityMatTempalte, typeof(Material), false);
        motionBlitMatTemplate = (Material)EditorGUILayout.ObjectField("Motion Material", motionBlitMatTemplate, typeof(Material), false);

        genParams.SampleMethod = (ParticleCityGenSampleMethod)EditorGUILayout.EnumPopup("Sampler", genParams.SampleMethod);

        if (genParams.SampleMethod == ParticleCityGenSampleMethod.Volume)
        {
            genParams.SamplePerCubeUnit = EditorGUILayout.FloatField("Sample Per Cube Unit", genParams.SamplePerCubeUnit);
        }
        else if (genParams.SampleMethod == ParticleCityGenSampleMethod.Surface)
        {
            genParams.SamplePerSquareUnit = EditorGUILayout.FloatField("Sample Per Square Unit", genParams.SamplePerSquareUnit);
            genParams.TriangleEdgeSamplePerUnit = EditorGUILayout.FloatField("Triangle Edge Sample Per Unit", genParams.TriangleEdgeSamplePerUnit);
        }

        shouldGenDebugParticles = EditorGUILayout.Toggle("Debug Particle", shouldGenDebugParticles);
        shouldGenTextures = EditorGUILayout.Toggle("Build Textures", shouldGenTextures);
        shouldGenMesh = EditorGUILayout.Toggle("Generate Mesh", shouldGenMesh);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate")) {
            generateParticles();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Clear")) 
        {
            clearParticles();
        }

        EditorGUILayout.Space();

        genParamsToLoad = (ParticleCityGenParams)EditorGUILayout.ObjectField("Params", genParamsToLoad, typeof(ParticleCityGenParams), false);
        if (GUILayout.Button("Load Settings") && genParamsToLoad != null)
        {
            genParams = Instantiate(genParamsToLoad);
        }

        EditorGUILayout.EndVertical();
    }

    private void generateParticles() {
        loadGeneratedAssets();

        if (genParams.SampleMethod == ParticleCityGenSampleMethod.Volume)
        {
            var colliders = Selection.GetFiltered(typeof(Collider), SelectionMode.Deep).Select((obj) => (Collider)obj).ToArray();

            if (colliders.Length == 0) {
                Debug.LogError("Particle City Gen: No model selected");
                return;
            }

            samplePoints(colliders);
        }
        else if (genParams.SampleMethod == ParticleCityGenSampleMethod.Surface)
        {
            MeshFilter[] meshFilters = Selection.GetFiltered<MeshFilter>(SelectionMode.Deep);
            if (meshFilters.Length == 0)
            {
                Debug.LogError("Particle City Gen: No model selected");
                return;
            }
            sampleTriangles(meshFilters);
        }

        if (shouldGenDebugParticles) {
            genDebugParticles();
        }

        if (shouldGenTextures || shouldGenMesh)
        {
            AssetDatabase.CreateAsset(Instantiate(genParams), getPath("Params.asset"));     
        }

        if (shouldGenTextures) 
        {
            genPositionTexture();
        }

        if (shouldGenMesh) 
        {
            genMesh();
        }

        AssetDatabase.SaveAssets();

        if (particleCity != null)
        {
            Selection.activeObject = particleCity;
        }
    }

    private void samplePoints(Collider[] colliders) {
        Debug.Log("Particle City Gen: Samping particles");
        Debug.Log("Found " + colliders.Length + " colliders");

        points = new List<Vector3>(genParams.TextureWidth * genParams.TextureHeight);

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

            Debug.DrawLine(collider.bounds.min, collider.bounds.max, Color.red, 5);

            float volume = Mathf.Abs(boundsDelta.x * boundsDelta.y * boundsDelta.z);
            int sampleCount = (int)(volume * genParams.SamplePerCubeUnit);

            totalSampleCount += sampleCount;
            // Debug.Log("Sample count: " + sampleCount + ", Total: " + totalSampleCount);

            for (int j = 0; j < sampleCount; j++) {
                Vector3? p = GeometryUtils.SampleCollider(collider);
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

        if (points.Count > genParams.TextureWidth * genParams.TextureHeight) {
            Debug.LogError("Particle City Gen: Too many points for specified texture size");
        }

        EditorUtility.ClearProgressBar();
    }

    private void sampleTriangles(MeshFilter[] meshFilters) {
        Debug.Log("Particle City Gen: Samping particles");
        Debug.Log("Found " + meshFilters.Length + " mesh filters");

        points = new List<Vector3>(genParams.TextureWidth * genParams.TextureHeight);

        int totalSampleCount = 0;

        bool cancel = false;

        for (int i = 0; i < meshFilters.Length; i++) {
            Mesh mesh = meshFilters[i].sharedMesh;
            Transform meshTransform = meshFilters[i].transform;

            if (cancel)
            {
                break;
            }

            for (int j = 0; j < mesh.triangles.Length; j += 3)
            {
                Vector3 p0 = mesh.vertices[mesh.triangles[j]];
                Vector3 p1 = mesh.vertices[mesh.triangles[j + 1]];
                Vector3 p2 = mesh.vertices[mesh.triangles[j + 2]];

                Vector3 v1 = p1 - p0;
                Vector3 v2 = p2 - p0;

                // Sample interior
                float area = Vector3.Cross(v1, v2).magnitude / 2.0f;
                int sampleCount = (int)(area * genParams.SamplePerSquareUnit);
                totalSampleCount += sampleCount;

                for (int k = 0; k < sampleCount; k++)
                {
                    Vector3? p = GeometryUtils.SampleTriangle(p0, v1, v2);
                    if (p.HasValue)
                    {
                        Vector3 globalPos = meshTransform.TransformPoint(p.Value);
                        points.Add(globalPos);
                    }
                    else
                    {
                        break;
                    }
                }

                // Sample edge
                float circumstance = v1.magnitude + v2.magnitude + (v2 - v1).magnitude;
                int edgeSampleCount = (int) (circumstance * genParams.TriangleEdgeSamplePerUnit);
                totalSampleCount += edgeSampleCount;

                for (int k = 0; k < edgeSampleCount; k++)
                {
                    Vector3 p = GeometryUtils.SampleTriangleEdge(p0, v1, v2);
                    Vector3 globalPos = meshTransform.TransformPoint(p);
                    points.Add(globalPos);
                }
            }

            int estimatedTotal = (i == 0) ? 0 : totalSampleCount / i * meshFilters.Length; 
            cancel = EditorUtility.DisplayCancelableProgressBar("Samping Particles", "Sampled Count " + totalSampleCount + ", estimated total " + estimatedTotal, (float)i / meshFilters.Length);
        }

        Debug.Log("Total sample count: " + totalSampleCount);
        Debug.Log(points.Count + " points sampled");

        if (points.Count > genParams.TextureWidth * genParams.TextureHeight) {
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
        if (genParams.TextureWidth * genParams.TextureHeight < points.Count) {
            Debug.LogError("Texture is too small to hold " + points.Count + " points.");
        }

        Debug.Log("Building position texture...");

        // TODO Perf: Generate raw data
        Color[] colors = new Color[genParams.TextureWidth * genParams.TextureHeight];
        for (var i = 0; i < points.Count; i++) {
            colors[i] = new Color(points[i].x, points[i].y, points[i].z);
        }

        positionTexture = new Texture2D(genParams.TextureWidth, genParams.TextureHeight, TextureFormat.RGBAFloat, false, true);
        positionTexture.anisoLevel = 1;
        positionTexture.filterMode = FilterMode.Point;
        positionTexture.SetPixels(colors);
        positionTexture.Apply();

        AssetDatabase.CreateAsset(positionTexture, getPath("ParticlePositions.asset"));

        // Create material
        string templatePath = AssetDatabase.GetAssetPath(cityMatTempalte);
        string newCityMatPath = getPath("ParticleCity.mat");
        AssetDatabase.CopyAsset(templatePath, newCityMatPath);
        Material particleCityGenMat = AssetDatabase.LoadAssetAtPath<Material>(newCityMatPath);
        particleCityGenMat.SetTexture("_PositionTex", positionTexture);

        templatePath = AssetDatabase.GetAssetPath(motionBlitMatTemplate);
        string newMotionPath = getPath("ParticleMotionBlit.mat");
        AssetDatabase.CopyAsset(templatePath, newMotionPath);
        Material particleMotionBlitMat = AssetDatabase.LoadAssetAtPath<Material>(newMotionPath);
        particleMotionBlitMat.SetTexture("_BasePositionTex", positionTexture);

        Debug.Log("Positions texture saved to " + getPath("ParticlePositions.asset"));
    }

    private void genMesh() {

        int pointCount = points.Count;

        // https://github.com/keijiro/KvantStream/blob/master/Assets/Kvant/Stream/Stream.cs CreateMesh

        int Nx = genParams.TextureWidth;
        int Ny = ceiling(pointCount, genParams.TextureWidth); // int ceiling

        Debug.Log("Creating grid mesh " + Nx + "x" + Ny + "...");

        // Create vertex arrays.
        var vertexArray = new Vector3[Nx * Ny];
        var uvArray = new Vector2[Nx * Ny];

        var index = 0;
        for (var x = 0; x < Nx; x++) {
            for (var y = 0; y < Ny; y++) {
                vertexArray[index] = new Vector3(x, 0, y);

                var u = (float)x / genParams.TextureWidth;
                var v = (float)y / genParams.TextureHeight;
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
			// MeshUtility.Optimize(meshes[i]);

            // Avoid being culled.
            meshes[i].bounds = new Bounds(Vector3.zero, Vector3.one * 100000);

            AssetDatabase.CreateAsset(meshes[i], getPath(string.Format("Mesh {0}.asset", i)));
        }

        // Create Prefab
        particleCity = new GameObject(genParams.GroupName + "_ParticleCity", typeof(ParticleMotion));
        var particleMotion = particleCity.GetComponent<ParticleMotion>();
        particleMotion.BasePositionTexture = positionTexture;
        particleMotion.ParticleMotionBlitMaterialPrefab = AssetDatabase.LoadAssetAtPath<Material>(getPath("ParticleMotionBlit.mat"));
        // particleMotion.LeftHand = GameObject.Find("/[CameraRig]/Controller (left)").transform;
        // particleMotion.RightHand = GameObject.Find("/[CameraRig]/Controller (right)").transform;

        for (int i = 0; i < meshes.Length; i++) {
            GameObject meshObject = new GameObject(genParams.GroupName + "_Mesh" + i, typeof(MeshFilter), typeof(MeshRenderer));
            meshObject.transform.parent = particleCity.transform;

            meshObject.GetComponent<MeshFilter>().mesh = meshes[i];
            meshObject.GetComponent<MeshRenderer>().material = AssetDatabase.LoadAssetAtPath<Material>(getPath("ParticleCity.mat"));
        }

        // PrefabUtility.CreatePrefab(getPath("ParticleCityPrefab.prefab"), particleCity);
        PrefabUtility.SaveAsPrefabAsset(particleCity, getPath("ParticleCityPrefab.prefab"));

        Debug.Log("Prefab saved to " + getPath("ParticleCityPrefab.prefab"));
    }

    private void clearParticles() 
    {
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

        Directory.Delete(Path.Combine("Assets/ParticleCityGen/", genParams.GroupName).Replace('\\', '/'), true);
        AssetDatabase.Refresh();
    }

    private void loadGeneratedAssets() 
    {
        // Try to load generated assets
        positionTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(getPath("ParticlePositions.asset"));
        particleCity = GameObject.Find("/" + genParams.GroupName + "_ParticleCity");
        debugParticles = GameObject.Find("/DebugParticles");
    }

    private static int ceiling(int a, int b) {
        return 1 + (int) ((a - 1) / b);
    }

    private string getPath(string assetName, bool createDir = true)
    {
        string dir = Path.Combine("Assets/ParticleCityGen/", genParams.GroupName);
        if (createDir && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string path = Path.Combine(dir, genParams.GroupName + "_" + assetName);
        return path.Replace('\\', '/');
    }
}
