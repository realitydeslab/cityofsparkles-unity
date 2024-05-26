using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using System.Runtime.Remoting.Channels;
using Newtonsoft.Json;
using ParticleCities;
using SQLite4Unity3d;
using TwitterViz.DataModels;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(TwitterDatabase))]
public class TwitterManager : MonoBehaviour
{
    private static TwitterManager instance;

    public static TwitterManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TwitterManager>();
            }

            return instance;
        }
    }

    public int MaxTweets = 100;
    public TweetComponent TweetObjectPrefab;
    public float SpawnInterval = 1;
    // TODO: Wwise
    //public AkAmbient BgmAkAmbient;
    public float HeightRangeOnGround = 200;
    public float MinHeightAboveGround = 30;

    [Header("Debugging")]
    public float PositiveRatio;

    [Header("Internal")]
    public TwitterDatabase Database;

    private Dictionary<int, TweetComponent> tweetsSpawned = new Dictionary<int, TweetComponent>();
    private Dictionary<int, SpawnRequest> tweetsToSpawn = new Dictionary<int, SpawnRequest>();
    private HashSet<int> tweetsToDelete = new HashSet<int>();

    private float lastSpawnTime;

    private Collider[] boundingColliders;
    private MapModel mapModel;

    private Sentiment previousSentiment;


    void Awake()
    {
        boundingColliders = GetComponents<Collider>();
        Database = GetComponent<TwitterDatabase>();
    }

    void Start()
    {
        mapModel = GetComponentInParent<MapModel>();
    }

    void Update()
    {
        if (Time.time - lastSpawnTime > SpawnInterval)
        {
            spawnIfNeeded();
            lastSpawnTime = Time.time;
        }
    }

    public void ClearAll()
    {
        tweetsToSpawn.Clear();
        tweetsToDelete.Clear();
        tweetsToDelete.UnionWith(tweetsSpawned.Keys);
    }

    public void Enqueue(IList<SpawnRequest> requests)
    {
        for (int i = 0; i < requests.Count; i++)
        {
            SpawnRequest r = requests[i];
            tweetsToDelete.Remove(r.Data.id);
            if (!tweetsSpawned.ContainsKey(r.Data.id))
            {
                tweetsToSpawn.Add(r.Data.id, r);
            }
        }
    }

    public void Enqueue(IList<TwitterDatabase.DBTweet> tweets)
    {
        for (int i = 0; i < tweets.Count; i++)
        {
            TwitterDatabase.DBTweet t = tweets[i];
            tweetsToDelete.Remove(t.id);
            if (!tweetsSpawned.ContainsKey(t.id))
            {
                tweetsToSpawn.Add(t.id, new SpawnRequest { Data = t });
            }
        }
    }

    private void spawnIfNeeded()
    {
        // TODO: DELETE ME
        //Debug.Log($"[TwitterManager] spawnIfNeeded tweetsToDelete.Count: {tweetsToDelete.Count}, tweetsToSpawn.Count: {tweetsToSpawn.Count}");

        // Remove one
        if (tweetsToDelete.Count > 0)
        {
            int index = tweetsToDelete.First();
            tweetsToDelete.Remove(index);
            
            if (tweetsSpawned.ContainsKey(index))
            {
                if (tweetsSpawned[index] != null && tweetsSpawned[index].State == TweetComponent.TweetState.Idle)
                {
                    tweetsSpawned[index].Finish();
                }

                // Destroy(tweetsSpawned[index].gameObject);
                tweetsSpawned.Remove(index);
            }
        }

        // Add one
        if (tweetsToSpawn.Count > 0)
        {
            var entry = tweetsToSpawn.First();
            int id = entry.Key;
            TwitterDatabase.DBTweet dbTweet = entry.Value.Data;
            tweetsToSpawn.Remove(entry.Key);
            
            tweetsToDelete.Remove(entry.Key);
            if (tweetsSpawned.ContainsKey(id))
            {
                tweetsSpawned[entry.Key].Finish();
            }

            Tweet tweet = new Tweet(dbTweet);

            Vector3? position = entry.Value.Source == null ? null : entry.Value.Source.GetPosition(entry.Value.Data);
            bool geoPos = false;

            if (!position.HasValue && tweet.Coordinates != null)
            {
                Vector3 candidatePos = mapModel.EarthToUnityWorld(tweet.Coordinates.Data[1], tweet.Coordinates.Data[0], 0);

                if (insideColliders(candidatePos, boundingColliders))
                {
                    // Sample height
                    candidatePos.y = sampleHeight(candidatePos);
                    position = candidatePos;
                    geoPos = true;
                }
            }

            if (!position.HasValue)
            {
                position = sample();
            }

            TweetComponent tweetObj = Instantiate(TweetObjectPrefab, transform);
            tweetObj.name = string.Format("Tweet_{0:F1}", tweet.Sentiment.Polarity);
            tweetObj.Tweet = tweet;
            tweetObj.Text = tweet.Text;
            tweetObj.Sentiment = tweet.Sentiment.Polarity;
            tweetObj.SpawnSource = entry.Value.Source;
            if (tweetObj.SpawnSource != null)
            {
                tweetObj.SpawnSource.OnTweetSpawned(tweetObj);
                tweetObj.GetComponent<GuidingLight>().MusicSync = tweetObj.SpawnSource.MusicSync;
            }

            // Spawn to actual geo location
            if (geoPos)
            {
                GeoObject geoObject = tweetObj.gameObject.AddComponent<GeoObject>();
                geoObject.SetWorldPosition(position.Value);

                // geoObject.SetGeoLocation(tweet.Coordinates.Data[1], tweet.Coordinates.Data[0], randomPosition.y);
            }
            else
            {
                tweetObj.transform.position = position.Value;
            }

            tweetsSpawned.Add(id, tweetObj);
        }

        // Update ratio
        // TODO: optimize
        /*
        int positive = 0;
        int negative = 0;
        foreach (TweetComponent tweet in tweetsSpawned.Values)
        {
            if (tweet.Sentiment > 0)
            {
                positive++;
            }
            else if (tweet.Sentiment < 0)
            {
                negative++;
            }
        }
        if (positive == 0 && negative == 0)
        {
            PositiveRatio = 1;
        }
        else
        {
            PositiveRatio = (float)positive / ((float)positive + (float)negative);
        }

        AkSoundEngine.SetRTPCValue("SentimentRatio", PositiveRatio, BgmAkAmbient.gameObject);
        */
    }

    private float sampleHeight(Vector3 position)
    {
        float bottom, top;
        if (CityStructure.Instance.HeightMap.GetHeightRange(position, out bottom, out top))
        {
            if (top > bottom + HeightRangeOnGround)
            {
                // More likelihood on higher positions
                // return bottom + MinHeightAboveGround + Mathf.Pow(Random.value, -4) * (top - bottom - MinHeightAboveGround);
                return bottom + HeightRangeOnGround + Mathf.Pow(Random.value, 0.25f) * (top - bottom - HeightRangeOnGround);
            }
            else
            {
                top = Mathf.Max(bottom + HeightRangeOnGround, top);
                return Random.Range(bottom + MinHeightAboveGround, top);
            }
        }
        else
        {
            bottom = CityStructure.Instance.HeightMap.Bounds.min.y;
            top = bottom + HeightRangeOnGround;
            return Random.Range(bottom + MinHeightAboveGround, top);
        }
    }

    private Vector3 sample() {
        // TODO: Better sampling

        int index = (int)Random.Range(0, boundingColliders.Length);
        Collider collider = boundingColliders[index];

        var bounds = collider.bounds;

        float x = Random.Range(bounds.min.x, bounds.max.x);
        float z = Random.Range(bounds.min.z, bounds.max.z);
        var p = new Vector3(x, 0, z);

        p.y = sampleHeight(p);

        return p;
    }

    private static bool insideColliders(Vector3 point, Collider[] colliders)
    {
        foreach (Collider c in colliders)
        {
            if (c.bounds.Contains(point))
            {
                return true;
            }
        }

        return false;
    }

    void OnDrawGizmosSelected()
    {
        HeightMap heightMap = FindObjectOfType<HeightMap>();

        Gizmos.color = Color.red;

        Vector3 center = new Vector3(
            heightMap.Bounds.center.x,
            heightMap.Bounds.min.y + (HeightRangeOnGround + MinHeightAboveGround) / 2,
            heightMap.Bounds.center.z
        );
        Vector3 size = new Vector3(
            heightMap.Bounds.size.x,
            HeightRangeOnGround - MinHeightAboveGround,
            heightMap.Bounds.size.z
        );
        Gizmos.DrawWireCube(center, size);
    }

    public struct SpawnRequest
    {
        public TwitterDatabase.DBTweet Data { get; set; }
        public SpawnSourceNode Source { get; set; }
    }
}
