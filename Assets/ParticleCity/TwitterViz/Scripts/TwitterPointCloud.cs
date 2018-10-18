using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AmberGarage.Trajen;
using ParticleCities;
using UnityEngine;
using WanderUtils;

public class TwitterPointCloud : MonoBehaviour
{
    public int LimitPerQuery = 10;
    public float Radius = 100;

    [Header("Internal")]
    public TwitterDatabase TwitterDatabase;
    public MapModel MapModel;
    public HeightMap HeightMap;

    private Dictionary<SentimentSpawnNode.Sentiment, FlannPointCloud> flann = new Dictionary<SentimentSpawnNode.Sentiment, FlannPointCloud>();
    private List<FlannPointCloud.QueryResult> nearbyPoints = new List<FlannPointCloud.QueryResult>();

    void Start()
    {
        TwitterDatabase = transform.parent.GetComponentInChildren<TwitterDatabase>();
        MapModel = GetComponentInParent<MapModel>();
        HeightMap = transform.parent.GetComponentInChildren<HeightMap>();
    }

    void Update()
    {
        SentimentSpawnNode.Sentiment currentSentiment = ParticleCity.Current.SentimentForRandomTweet;
        if (currentSentiment == SentimentSpawnNode.Sentiment.Unspecified)
        {
            return;
        }

        if (currentSentiment != SentimentSpawnNode.Sentiment.Unspecified && !flann.ContainsKey(currentSentiment))
        {
            buildForSentiment(currentSentiment);
        }

        updateNearbyPoints(currentSentiment);
    }

    void OnDestroy()
    {
        foreach (var entry in flann)
        {
            entry.Value.Dispose();
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 camCenter = InputManager.Instance.CenterCamera.transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(camCenter, Radius);

        Gizmos.color = Color.red;

        for (int i = 0; i < nearbyPoints.Count; i++)
        {
            Gizmos.DrawSphere(new Vector3(nearbyPoints[i].Position.x, camCenter.y, nearbyPoints[i].Position.y), 5);
        }
    }

    private void buildForSentiment(SentimentSpawnNode.Sentiment sentiment)
    {
        Debug.Log("Building Flann point cloud for sentiment " + sentiment);
        IList<TwitterDatabase.DBTweetPoint> points = TwitterDatabase.QueryForPointCloud(sentiment);
        Debug.Log("Found " + points.Count + " points from database. ");

        flann[sentiment] = new FlannPointCloud();
        flann[sentiment].LoadData(points, MapModel);

        Debug.Log("Flann point cloud built. ");
    }

    private void updateNearbyPoints(SentimentSpawnNode.Sentiment sentiment)
    {
        Vector3 center = InputManager.Instance.CenterCamera.transform.position;
        nearbyPoints.Capacity = LimitPerQuery;
        flann[sentiment].Query(center.GroundProjection2d(), Radius, LimitPerQuery, nearbyPoints);
        Debug.Log("Nearby points: " + nearbyPoints.Count);
    }
}

public class FlannPointCloud : IDisposable
{
    public struct QueryResult
    {
        public int DbId { get; set; }
        public Vector2 Position { get; set; }
    }

    private IntPtr flann;
    private IList<TwitterDatabase.DBTweetPoint> points;
    private float[] rawData;
    private int[] indices = new int[0];

    [DllImport("FlannWrapper")]
    private static extern IntPtr CreateFlannPointCloud(float[] rawData, int length);

    [DllImport("FlannWrapper")]
    private static extern void DeleteFlannPointCloud(IntPtr flann);

    [DllImport("FlannWrapper")]
    private static extern int QueryFlannPointCloud(IntPtr flann, float x, float y, float radius, int limit, [In, Out] int[] indices);

    public void LoadData(IList<TwitterDatabase.DBTweetPoint> points, MapModel mapModel)
    {
        this.points = points;

        // Convert to Flann data
        rawData = new float[points.Count * 2];
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 p = mapModel.EarthToUnityWorld(points[i].latitude, points[i].longitude, 0);
            rawData[i * 2] = p.x;
            rawData[i * 2 + 1] = p.z;
        }

        flann = CreateFlannPointCloud(rawData, rawData.Length);
    }

    public void Query(Vector2 center, float radius, int limit, IList<QueryResult> queryResult)
    {
        if (indices.Length != limit)
        {
            indices = new int[limit];
        }
        queryResult.Clear();

        int count = QueryFlannPointCloud(flann, center.x, center.y, radius, limit, indices);
        for (int i = 0; i < count; i++)
        {
            int index = indices[i];
            queryResult.Add(new QueryResult
            {
                DbId = points[index].id,
                Position = new Vector2(rawData[index * 2], rawData[index * 2 + 1])
            });
        }
    }

    public void Dispose()
    {
        if (flann != IntPtr.Zero)
        {
            DeleteFlannPointCloud(flann);            
        }
    }
}
