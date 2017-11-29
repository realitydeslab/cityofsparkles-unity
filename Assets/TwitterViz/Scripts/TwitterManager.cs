using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using Newtonsoft.Json;
using TwitterViz.DataModels;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Collider))]
public class TwitterManager : MonoBehaviour
{
    public enum Sentiment
    {
        Unspecified = 0,
        Positive,
        Negative
    }

    public string SourceAsset = "tweets";
    public int MaxTweets = 100;
    public TweetComponent TweetObjectPrefab;
    public Sentiment PreferredSentiment;
    public float SpawnInterval = 1;
    public AkAmbient BgmAkAmbient;

    [Header("Debugging")]
    public float PositiveRatio;

    private Tweet[] tweets;

    private Dictionary<int, TweetComponent> tweetsSpawned = new Dictionary<int, TweetComponent>();
    private HashSet<int> tweetsToSpawn = new HashSet<int>();
    private HashSet<int> tweetsToDelete = new HashSet<int>();

    private float lastSpawnTime;

    private Collider boundingCollider;
    private Sentiment previousSentiment;

    private int triggerCount;

    void Awake()
    {
        boundingCollider = GetComponent<Collider>();

        TextAsset tweetsAsset = Resources.Load<TextAsset>(SourceAsset);
        if (tweetsAsset != null)
        {
            tweets = JsonConvert.DeserializeObject<Tweet[]>(tweetsAsset.text);

            // Already sorted
            // Array.Sort(tweets, (x, y) => x.Sentiment.Polarity.CompareTo(y.Sentiment.Polarity));

            Debug.Log("Loaded " + tweets.Length + " tweets.");
        }
    }

    void Update()
    {
        if (PreferredSentiment != previousSentiment)
        {
            updateForSentiment();
            previousSentiment = PreferredSentiment;
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
        if (triggerCount % 5 == 0)
        {
            PreferredSentiment = (PreferredSentiment == Sentiment.Positive)
                ? Sentiment.Negative
                : Sentiment.Positive;

        }
    }

    private void updateForSentiment()
    {
        AkSoundEngine.SetState("Sentiment", PreferredSentiment.ToString());

        // Find new set of tweets
        List<int> NewIndices = new List<int>(MaxTweets);
        switch (PreferredSentiment)
        {
            case Sentiment.Positive:
                for (int i = tweets.Length - 1; i >= 0; i--)
                {
                    if (tweets[i].Sentiment.Polarity <= 0)
                    {
                        break;
                    }

                    if (NewIndices.Count >= MaxTweets)
                    {
                        break;
                    }

                    NewIndices.Add(i);
                }
                break;
            case Sentiment.Negative:
                for (int i = 0; i < tweets.Length; i++)
                {
                    if (tweets[i].Sentiment.Polarity >= 0)
                    {
                        break;
                    }

                    if (NewIndices.Count >= MaxTweets)
                    {
                        break;
                    }

                    NewIndices.Add(i);
                }
                break;
            default:
                break;
        }

        // Diff
        tweetsToSpawn.Clear();
        tweetsToDelete.Clear();
        tweetsToDelete.UnionWith(tweetsSpawned.Keys);

        foreach (int i in NewIndices)
        {
            tweetsToDelete.Remove(i);
            if (!tweetsSpawned.ContainsKey(i))
            {
                tweetsToSpawn.Add(i);
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
            int index = tweetsToSpawn.First();
            tweetsToSpawn.Remove(index);

            if (!tweetsSpawned.ContainsKey(index))
            {
                Tweet tweet = tweets[index];
                Vector3 randomPosition = sample(boundingCollider);

                TweetComponent tweetObj = Instantiate(TweetObjectPrefab, transform);
                tweetObj.name = string.Format("Tweet_{0:F1}", tweet.Sentiment.Polarity);
                tweetObj.Tweet = tweet;
                tweetObj.Text = tweet.Text;
                tweetObj.Sentiment = tweet.Sentiment.Polarity;

                // Spawn to actual geo location
                if (tweet.Coordinates != null)
                {
                    GeoObject geoObject = tweetObj.gameObject.AddComponent<GeoObject>();
                    geoObject.SetGeoLocation(tweet.Coordinates.Data[1], tweet.Coordinates.Data[0], randomPosition.y);
                }

                tweetsSpawned.Add(index, tweetObj);
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

    private static Vector3 sample(Collider collider) {
        // TODO: Better sampling
        // TODO: Check if inside collider

        var bounds = collider.bounds;

        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        float z = Random.Range(bounds.min.z, bounds.max.z);
        var p = new Vector3(x, y, z);

        return p;
    }
}
