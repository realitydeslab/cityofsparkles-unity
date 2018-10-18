using System;
using System.Collections;
using System.Collections.Generic;
using SQLite4Unity3d;
using TwitterViz.DataModels;
using UnityEngine;
using Sentiment = SentimentSpawnNode.Sentiment;

public class TwitterDatabase : MonoBehaviour {

    public class DBTweetPoint
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }

        public double latitude { get; set; }
        public double longitude { get; set; }
    }

    public class DBTweet
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        
        public string clean_text { get; set; }

        public double latitude { get; set; }
        public double longitude { get; set; }

        public double sentiment_positive { get; set; }
        public double sentiment_negative { get; set; }
        public double sentiment_polarity { get; set; }

        public string tag { get; set; }
        public DateTime last_access { get; set; }

        public string username { get; set; }
        public string created_at { get; set; }

        public override string ToString()
        {
            return string.Format("[{0:0.00}, {1:0.00}] {2}", sentiment_positive, sentiment_negative, clean_text);
        }

        public bool IsDummy
        {
            get { return id < 0; }
        }

        private static int emptyPlaceholderIdCount = -1;
        public static DBTweet EmptyPlaceholder()
        {
            DBTweet result = new DBTweet
            {
                id = emptyPlaceholderIdCount,
                clean_text = "",
                last_access = DateTime.UtcNow,
                created_at = DateTime.UtcNow.ToLongTimeString(),
            };
            emptyPlaceholderIdCount--;
            return result;
        }
    }

    public string Database = "twitter_sf.db";

    private SQLiteConnection dbConnection;

    public IList<DBTweet> QueryTweetsForSentiment(Sentiment sentiment, int limit)
    {
        checkConnection();
        string query;

        switch (sentiment)
        {
            case Sentiment.Neutral:
            default:
                query = "SELECT * FROM tweets ORDER BY RANDOM() LIMIT ?";
                break;

            case Sentiment.Happy:
                query = "SELECT * FROM tweets WHERE sentiment_positive > 0.6 AND NOT (clean_text LIKE '%wish%' OR clean_text lIKE '%hope%') ORDER BY RANDOM() LIMIT ?";
                break;

            case Sentiment.Sad:
                // query = "SELECT * FROM tweets WHERE sentiment_negative > 0.5 ORDER BY RANDOM() LIMIT ?";
                return QueryForTags("fuck", limit);

            case Sentiment.Wish:
                // query = "SELECT * FROM tweets WHERE sentiment_positive > 0.3 AND (clean_text LIKE '%wish%' OR clean_text lIKE '%hope%') ORDER BY RANDOM() LIMIT ?";
                return QueryForTags("positive", limit);

        }

        List<DBTweet> results = dbConnection.Query<DBTweet>(query, limit);
        RecordLastAccessTime(results);

        return results;
    }

    public DBTweet QueryOne()
    {
        checkConnection();
        string query = "SELECT * FROM tweets WHERE sentiment_negative > 0.5 ORDER BY RANDOM() LIMIT 1";
        List<DBTweet> result = dbConnection.Query<DBTweet>(query);
        RecordLastAccessTime(result);
        return result.Count == 0 ? null : result[0];
    }

    public IList<DBTweet> QueryForTags(string tag, int limit)
    {
        checkConnection();
        string query = "SELECT * FROM tags ta INNER JOIN tweets tw ON ta.id = tw.id WHERE ta.tag = ? ORDER BY last_access LIMIT ?";
        List<DBTweet> result = dbConnection.Query<DBTweet>(query, tag, limit);
        RecordLastAccessTime(result);

        return result;
    }

    
    public IList<DBTweetPoint> QueryForPointCloud(Sentiment sentiment)
    {
        checkConnection();
        string query;
        switch (sentiment)
        {
            case Sentiment.Neutral:
            default:
                query = "SELECT id, latitude, longitude FROM tweets_random WHERE " +
                                                 "(sentiment_polarity < 0.5 AND sentiment_polarity > -0.5) OR " +
                                                 "(sentiment_positive < 0.5 AND sentiment_negative < 0.5)";
                break;

            case Sentiment.Happy:
            case Sentiment.Wish:
                query = "SELECT id, latitude, longitude FROM tweets_random WHERE " +
                                                 "sentiment_polarity > 0.8 OR " +
                                                 "sentiment_positive > 0.8";
                break;

            case Sentiment.Sad:
                query = "SELECT id, latitude, longitude FROM tweets_random WHERE " +
                                                 "sentiment_polarity < -0.5 OR " +
                                                 "sentiment_negative > 0.5";
                break;
        }

        return dbConnection.Query<DBTweetPoint>(query);
    }


    public void RecordLastAccessTime(string[] ids)
    {
        checkConnection();

        string query = string.Format("UPDATE tweets SET last_access = ? WHERE id IN ({0})", string.Join(", ", ids));
        dbConnection.Execute(query, DateTime.UtcNow);
    }

    public void RecordLastAccessTime(IList<DBTweet> tweets)
    {
        string[] ids = new string[tweets.Count];
        for (int i = 0; i < tweets.Count; i++)
        {
            ids[i] = tweets[i].id.ToString();
        }

        RecordLastAccessTime(ids);
    }
    
    void Awake()
    {
        checkConnection();
    }

	void Start () 
	{
		
	}
	
	void Update () 
	{
	}

    private void checkConnection()
    {
	    if (dbConnection == null)
	    {
            Debug.Log("Connecting to DB");
            string dbPath = Application.dataPath + "/StreamingAssets/" + Database;
            dbConnection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite, true);
	    }
    }

    void OnDestroy()
    {
        if (dbConnection != null)
        {
            dbConnection.Close();
        }
    }
}
