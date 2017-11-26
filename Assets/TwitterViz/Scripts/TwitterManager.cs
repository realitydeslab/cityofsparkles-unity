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

    public int MaxTweets = 100;
    public TweetComponent TweetObjectPrefab;
    public Sentiment PreferredSentiment;
    public float SpawnInterval = 1;
    public AkAmbient BgmAkAmbient;

    [Header("Debugging")]
    private float PositiveRatio;

    private Tweets tweets;

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

        TextAsset tweetsAsset = Resources.Load<TextAsset>("tweets2");
        if (tweetsAsset != null)
        {
            tweets = JsonConvert.DeserializeObject<Tweets>(tweetsAsset.text);

            // TODO: Pre-sort
            Array.Sort(tweets.AllTweets, (x, y) => x.Sentiment.Polarity.CompareTo(y.Sentiment.Polarity));

            Debug.Log("Loaded " + tweets.AllTweets.Length + " tweets.");
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
                for (int i = tweets.AllTweets.Length - 1; i >= 0; i--)
                {
                    if (tweets.AllTweets[i].Sentiment.Polarity <= 0)
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
                for (int i = 0; i < tweets.AllTweets.Length; i++)
                {
                    if (tweets.AllTweets[i].Sentiment.Polarity >= 0)
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
                Tweet tweet = tweets.AllTweets[index];
                Vector3 position = sample(boundingCollider);
                TweetComponent tweetObj = Instantiate(TweetObjectPrefab, position, Quaternion.identity, transform);
                tweetObj.name = string.Format("Tweet_{0:F1}", tweet.Sentiment.Polarity);
                tweetObj.Tweet = tweet;
                tweetObj.Text = tweet.Text;
                tweetObj.Sentiment = tweet.Sentiment.Polarity;

                tweetsSpawned.Add(index, tweetObj);
            }
        }

        // Update ratio
        // TODO: optimize
        int positive = 0;
        int negative = 0;
        for (int i = 0; i < tweetsSpawned.Count; i++)
        {
            if (tweetsSpawned[i].Sentiment > 0)
            {
                positive++;
            }
            else if (tweetsSpawned[i].Sentiment < 0)
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
