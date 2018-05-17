using System.Collections;
using System.Collections.Generic;
using SQLite4Unity3d;
using TwitterViz.DataModels;
using UnityEngine;
using Sentiment = SentimentSpawnNode.Sentiment;

public class TwitterDatabase : MonoBehaviour {

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

    public string Database = "twitter_sf.db";

    private SQLiteConnection dbConnection;

    public List<DBTweet> QueryTweetsForSentiment(Sentiment sentiment, int limit)
    {
        string query;

        switch (sentiment)
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

        return dbConnection.Query<DBTweet>(query, limit);
    }

    public DBTweet QueryOne()
    {
        string query = "SELECT * FROM tweets WHERE sentiment_negative > 0.5 ORDER BY RANDOM() LIMIT 1";
        List<DBTweet> result = dbConnection.Query<DBTweet>(query);
        return result.Count == 0 ? null : result[0];
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
        checkConnection();
	}

    private void checkConnection()
    {
	    if (dbConnection == null)
	    {
            string dbPath = Application.dataPath + "/StreamingAssets/" + Database;
            dbConnection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite);
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
