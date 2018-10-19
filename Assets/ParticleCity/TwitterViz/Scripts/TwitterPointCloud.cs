using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AmberGarage.Trajen;
using ParticleCities;
using TwitterViz.DataModels;
using UnityEngine;
using UnityEngine.Profiling;
using WanderUtils;

public class TwitterPointCloud : MonoBehaviour
{
    // public TweetComponent RandomTweetPrefab;
    public TweetCanvas RandomTweetPrefab;

    public int LimitPerQuery = 10;
    public float Radius = 100;
    public float Cooldown = 5;
    public float AccessTimeCooldown = 30;
    public float MinMoveDistance = 100;
    public float HeightTolerance = 10;

    [Header("Internal")]
    public TwitterDatabase TwitterDatabase;
    public MapModel MapModel;
    public HeightMap HeightMap;

    private Dictionary<SentimentSpawnNode.Sentiment, FlannPointCloud> flann = new Dictionary<SentimentSpawnNode.Sentiment, FlannPointCloud>();
    private List<FlannPointCloud.QueryResult> nearbyPoints = new List<FlannPointCloud.QueryResult>();
    private float lastSpawnTime = 0;
    private Vector3 lastPlayerPosition;

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

        if ((Time.time - lastSpawnTime > Cooldown) && 
            (InputManager.Instance.PlayerTransform.position - lastPlayerPosition).sqrMagnitude > MinMoveDistance * MinMoveDistance )
        {

            Vector3 camPos = InputManager.Instance.CenterCamera.transform.position;
            float minHeight, maxHeight;
            if (HeightMap.GetHeightRange(camPos, out minHeight, out maxHeight) &&
                (camPos.y > minHeight - HeightTolerance) && (camPos.y < maxHeight + HeightTolerance)
            )
            {
                updateNearbyPoints(currentSentiment);

                if (spawnTweet())
                {
                    lastSpawnTime = Time.time;
                    lastPlayerPosition = InputManager.Instance.PlayerTransform.position;
                }
            }
        }

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
        Profiler.BeginSample("Flann Query");
        flann[sentiment].Query(center.GroundProjection2d(), Radius, LimitPerQuery, nearbyPoints);
        Profiler.EndSample();
    }

    private bool spawnTweet()
    {
        if (nearbyPoints.Count == 0)
        {
            return false;
        }

        Profiler.BeginSample("DB Query");
        TwitterDatabase.DBTweet dbTweet = TwitterDatabase.QueryForPointCloudQueryResult(nearbyPoints, AccessTimeCooldown);
        Profiler.EndSample();
        if (dbTweet == null)
        {
            return false;
        }

        Vector3 position = MapModel.EarthToUnityWorld(dbTweet.latitude, dbTweet.longitude, 0);

        if ( (InputManager.Instance.CenterCamera.transform.position.GroundProjection2d() - position.GroundProjection2d()).sqrMagnitude > Radius * Radius )
        {
            // Debug.LogWarning("Unexpected far away point queried from Flann point cloud. ");
            return false;
        }

        position.y = InputManager.Instance.CenterCamera.transform.position.y;

        Quaternion rotation = Quaternion.LookRotation( position - InputManager.Instance.CenterCamera.transform.position, Vector3.up );

        TweetCanvas tweetObj = Instantiate(RandomTweetPrefab, position, rotation, transform);
        tweetObj.Tweet = dbTweet;

        // Tweet tweet = new Tweet(dbTweet);
        // TweetComponent tweetObj = Instantiate(RandomTweetPrefab, position, Quaternion.identity, transform);
        // tweetObj.name = string.Format("Tweet_{0:F1}", tweet.Sentiment.Polarity);
        // tweetObj.Tweet = tweet;
        // tweetObj.Text = tweet.Text;
        // tweetObj.Sentiment = tweet.Sentiment.Polarity;
        // tweetObj.Trigger = true;
        return true;
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

    public void Query(Vector2 center, float radius, int limit, List<QueryResult> queryResult)
    {
        if (indices.Length != limit)
        {
            indices = new int[limit];
        }
        queryResult.Capacity = limit;
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
