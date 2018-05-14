using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
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
    public enum Sentiment
    {
        Unspecified = 0,
        Neutral,
        Happy,
        Sad,
        Wish
    }

    public int MaxTweets = 100;
    public TweetComponent TweetObjectPrefab;
    public Sentiment PreferredSentiment;
    public float SpawnInterval = 1;
    public AkAmbient BgmAkAmbient;
    public float HeightRangeOnGround = 200;
    public float MinHeightAboveGround = 30;

    [Header("Debugging")]
    public float PositiveRatio;

    private Dictionary<int, TweetComponent> tweetsSpawned = new Dictionary<int, TweetComponent>();
    private Dictionary<int, SpawnRequest> tweetsToSpawn = new Dictionary<int, SpawnRequest>();
    private HashSet<int> tweetsToDelete = new HashSet<int>();

    private float lastSpawnTime;

    private Collider[] boundingColliders;
    private MapModel mapModel;

    private Sentiment previousSentiment;
    private int triggerCount;

    private TwitterDatabase database;

    void Awake()
    {
        boundingColliders = GetComponents<Collider>();
        database = GetComponent<TwitterDatabase>();
    }

    void Start()
    {
        AkSoundEngine.SetState("RichSentimentTest", PreferredSentiment.ToString());

        mapModel = GetComponentInParent<MapModel>();
    }

    void Update()
    {
        if (PreferredSentiment != previousSentiment)
        {
            updateForSentiment();
            previousSentiment = PreferredSentiment;
            Debug.Log("Sentiment change: " + PreferredSentiment);
        }

        if (Time.time - lastSpawnTime > SpawnInterval)
        {
            spawnIfNeeded();
            lastSpawnTime = Time.time;
        }
    }

    public void RecordFirstTrigger(TweetComponent tweet)
    {
        triggerCount++;

        if (tweet.TargetSentiment == PreferredSentiment)
        {
            AkSoundEngine.SetState("RichSentimentTest", PreferredSentiment.ToString());
        }

        switch (PreferredSentiment)
        {
            case Sentiment.Neutral:
            default:
                if (triggerCount > 5)
                {
                    triggerCount = 0;
                    PreferredSentiment = Sentiment.Happy;
                }
                break;
            
            case Sentiment.Happy:
                if (triggerCount > 10)
                {
                    triggerCount = 0;
                    PreferredSentiment = Sentiment.Sad;
                }
                break;
            
            case Sentiment.Sad:
                if (triggerCount > 5)
                {
                    triggerCount = 0;
                    PreferredSentiment = Sentiment.Wish;
                }
                break;
            
            case Sentiment.Wish:
                if (triggerCount > 5)
                {
                    triggerCount = 0;
                    PreferredSentiment = Sentiment.Neutral;
                }
                break;
        }
    }

    private void updateForSentiment()
    {
        // Find new set of tweets
        List<TwitterDatabase.DBTweet> newTweets = database.QueryTweetsForSentiment(PreferredSentiment, MaxTweets);

        // Diff
        tweetsToSpawn.Clear();
        tweetsToDelete.Clear();
        tweetsToDelete.UnionWith(tweetsSpawned.Keys);

        foreach (TwitterDatabase.DBTweet tweet in newTweets)
        {
            tweetsToDelete.Remove(tweet.id);
            if (!tweetsSpawned.ContainsKey(tweet.id))
            {
                tweetsToSpawn.Add(tweet.id, new SpawnRequest
                {
                    Data = tweet
                });
            }
        }
    }

    private void spawnIfNeeded()
    {
        // Remove one
        if (tweetsToDelete.Count > 0)
        {
            int index = tweetsToDelete.First();
            tweetsToDelete.Remove(index);
            if (tweetsSpawned.ContainsKey(index))
            {
                tweetsSpawned[index].MarkForDestroy();
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

            if (!tweetsSpawned.ContainsKey(id))
            {
                Tweet tweet = new Tweet(dbTweet);

                Vector3? position = null;
                bool geoPos = false;

                if (entry.Value.Placeholder != null)
                {
                    position = entry.Value.Placeholder.transform.position;
                }
                else if (tweet.Coordinates != null)
                {
                    Vector3 candidatePos = mapModel.EarthToUnityWorld(tweet.Coordinates.Data[1], tweet.Coordinates.Data[0], 0);

                    if (insideColliders(candidatePos, boundingColliders))
                    {
                        // Sample height
                        candidatePos.y = sampleHeight(candidatePos);
                        position = candidatePos;
                        geoPos = true;
                    }
                    // Do not use position outside the bounding box
                }

                if (position == null)
                {
                    position = sample();
                }

                TweetComponent tweetObj = Instantiate(TweetObjectPrefab, transform);
                tweetObj.name = string.Format("Tweet_{0:F1}", tweet.Sentiment.Polarity);
                tweetObj.Tweet = tweet;
                tweetObj.Text = tweet.Text;
                tweetObj.Sentiment = tweet.Sentiment.Polarity;
                tweetObj.TargetSentiment = PreferredSentiment;

                if (entry.Value.Placeholder != null)
                {
                    entry.Value.Placeholder.transform.SetParent(tweetObj.transform);
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
        }

        // Update ratio
        // TODO: optimize
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
        // TODO: Check if inside collider

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

    private struct SpawnRequest
    {
        public TwitterDatabase.DBTweet Data { get; set; }
        public TweetPlaceholder Placeholder { get; set; }
    }
}
