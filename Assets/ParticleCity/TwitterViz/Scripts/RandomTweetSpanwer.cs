using ParticleCities;
using TwitterViz.DataModels;
using UnityEngine;
using System.Collections.Generic;
using WanderUtils;

public class RandomTweetSpanwer : MonoBehaviour
{

    public TweetComponent RandomTweetPrefab;

	void Update () {
	    if (InputManager.Instance.GetButtonDown(Button.B))
	    {
	        TweetComponent tweetObj = Instantiate(RandomTweetPrefab, InputManager.Instance.GetHand(HandType.Right).transform.position, Quaternion.identity, transform);

	        // IList<TwitterDatabase.DBTweet> dbTweets = TwitterManager.Instance.Database.QueryTweetsForSentiment(ParticleCity.Current.SentimentForRandomTweet, 1);
	        IList<TwitterDatabase.DBTweet> dbTweets = TwitterManager.Instance.Database.QueryForTags("city", 1);
	        if (dbTweets.Count > 0)
	        {
	            Tweet tweet = new Tweet(dbTweets[0]);
	            tweetObj.name = string.Format("Tweet_{0:F1}", tweet.Sentiment.Polarity);
	            tweetObj.Tweet = tweet;
	            tweetObj.Text = tweet.Text;
	            tweetObj.Sentiment = tweet.Sentiment.Polarity;
	            tweetObj.Trigger = true;
	        }
	    }
    }
}
