using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using Newtonsoft.Json;
using SQLite4Unity3d;
using TwitterViz.DataModels;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Collider))]
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

    public string Database = "twitter_sf.db";

    public int MaxTweets = 100;
    public TweetComponent TweetObjectPrefab;
    public Sentiment PreferredSentiment;
    public float SpawnInterval = 1;
    public AkAmbient BgmAkAmbient;

    [Header("Debugging")]
    public float PositiveRatio;

    private Dictionary<int, TweetComponent> tweetsSpawned = new Dictionary<int, TweetComponent>();
    private Dictionary<int, DBTweet> tweetsToSpawn = new Dictionary<int, DBTweet>();
    private HashSet<int> tweetsToDelete = new HashSet<int>();

    private float lastSpawnTime;

    private Collider[] boundingColliders;
    private MapModel mapModel;
    private Sentiment previousSentiment;

    private int triggerCount;

    private SQLiteConnection dbConnection;

    public class DBTweet
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        
        public string clean_text { get; set; }

        public double latitude { get; set; }
        public double longitude { get; set; }

        public double sentiment_positive { get; set; }
        public double sentiment_neutral { get; set; }
        public double sentiment_negative { get; set; }
        public double sentiment_mixed { get; set; }

        public override string ToString()
        {
            return string.Format("[{0:0.00}, {1:0.00}] {2}", sentiment_positive, sentiment_negative, clean_text);
        }
    }

    void Awake()
    {
        boundingColliders = GetComponents<Collider>();

        string dbPath = Application.dataPath + "/StreamingAssets/" + Database;
        dbConnection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite);
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

    void OnDestroy()
    {
        dbConnection.Close();    
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
        List<DBTweet> newTweets = queryTweetsForPreferredSentiment();

        // Diff
        tweetsToSpawn.Clear();
        tweetsToDelete.Clear();
        tweetsToDelete.UnionWith(tweetsSpawned.Keys);

        foreach (DBTweet tweet in newTweets)
        {
            tweetsToDelete.Remove(tweet.id);
            if (!tweetsSpawned.ContainsKey(tweet.id))
            {
                tweetsToSpawn.Add(tweet.id, tweet);
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
            DBTweet dbTweet = entry.Value;
            tweetsToSpawn.Remove(entry.Key);

            if (!tweetsSpawned.ContainsKey(id))
            {
                Tweet tweet = new Tweet(dbTweet);

                // Skip tweets outside bounding box
                if (tweet.Coordinates != null)
                {
                    Vector3 position = mapModel.EarthToUnityWorld(tweet.Coordinates.Data[1], tweet.Coordinates.Data[0], 0);
                    if (!insideColliders(position, boundingColliders))
                    {
                        spawnIfNeeded();
                        return;
                    }
                }
                Vector3 randomPosition = sample(boundingColliders);

                TweetComponent tweetObj = Instantiate(TweetObjectPrefab, transform);
                tweetObj.name = string.Format("Tweet_{0:F1}", tweet.Sentiment.Polarity);
                tweetObj.Tweet = tweet;
                tweetObj.Text = tweet.Text;
                tweetObj.Sentiment = tweet.Sentiment.Polarity;
                tweetObj.TargetSentiment = PreferredSentiment;

                // Spawn to actual geo location
                if (tweet.Coordinates != null)
                {
                    GeoObject geoObject = tweetObj.gameObject.AddComponent<GeoObject>();
                    
                    // TODO: Heightmap
                    geoObject.SetGeoLocation(tweet.Coordinates.Data[1], tweet.Coordinates.Data[0], randomPosition.y);
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

    private static Vector3 sample(Collider[] colliders) {
        // TODO: Better sampling
        // TODO: Check if inside collider

        int index = (int)Random.Range(0, colliders.Length);
        Collider collider = colliders[index];

        var bounds = collider.bounds;

        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        float z = Random.Range(bounds.min.z, bounds.max.z);
        var p = new Vector3(x, y, z);

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

    private List<DBTweet> queryTweetsForPreferredSentiment()
    {
        string query;

        switch (PreferredSentiment)
        {
            case Sentiment.Neutral:
            default:
                query = "SELECT * FROM tweets WHERE sentiment_neutral > 0.8 ORDER BY RANDOM() LIMIT ?";
                break;

            case Sentiment.Happy:
                query = "SELECT * FROM tweets WHERE sentiment_positive > 0.6 AND NOT (clean_text LIKE '%wish%' OR clean_text lIKE '%hope%') ORDER BY RANDOM() LIMIT ?";
                break;

            case Sentiment.Sad:
                query = "SELECT * FROM tweets WHERE sentiment_negative > 0.5 ORDER BY RANDOM() LIMIT ?";
                break;

            case Sentiment.Wish:
                query = "SELECT * FROM tweets WHERE sentiment_positive > 0.3 AND (clean_text LIKE '%wish%' OR clean_text lIKE '%hope%') ORDER BY RANDOM() LIMIT ?";
                break;

        }

        return dbConnection.Query<DBTweet>(query,  MaxTweets);
    }
}
