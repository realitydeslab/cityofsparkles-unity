using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SentimentSpawnNode : SpawnSourceNode {

    public enum Sentiment
    {
        Unspecified = 0,
        Neutral,
        Happy,
        Sad,
        Wish
    }
		
    public Sentiment PreferredSentiment = Sentiment.Neutral;

    [Header("Internal")] 
    public int TriggerCount = 0;

    private Sentiment previousSentiment;


    public override void OnEnable()
    {
        base.OnEnable();

        AkSoundEngine.SetState("RichSentimentTest", PreferredSentiment.ToString());

        GotoNext(); 
    }

    public override void OnTweetSpawned(TweetComponent tweet)
    {
        tweet.SpawnSourceUserData = PreferredSentiment;
    }

    public override void OnTweetRevealed(TweetComponent obj)
    {
        if ((Sentiment)obj.SpawnSourceUserData == PreferredSentiment)
        {
            AkSoundEngine.SetState("RichSentimentTest", PreferredSentiment.ToString());
        }
    }

    public override void OnTweetTriggered(TweetComponent tweet)
    {
        TriggerCount++;
    }

    public override void Update()
    {
        base.Update();
        if (PreferredSentiment != previousSentiment)
        {
            // Find new set of tweets
            IList<TwitterDatabase.DBTweet> newTweets = TwitterManager.Instance.Database.QueryTweetsForSentiment(PreferredSentiment, TwitterManager.Instance.MaxTweets);
            TwitterManager.SpawnRequest[] requests = new TwitterManager.SpawnRequest[newTweets.Count];
            for (int i = 0; i < newTweets.Count; i++)
            {
                requests[i] = new TwitterManager.SpawnRequest
                {
                    Data = newTweets[i],
                    Source = this
                };
            }

            TwitterManager.Instance.ClearAll();
            TwitterManager.Instance.Enqueue(requests);

            previousSentiment = PreferredSentiment;
            Debug.Log("Sentiment change: " + PreferredSentiment);
        }
        else
        {
            // switch (PreferredSentiment)
            // {
            //     case Sentiment.Neutral:
            //     default:
            //         if (TriggerCount > 3)
            //         {
            //             TriggerCount = 0;
            //             PreferredSentiment = Sentiment.Happy;
            //         }

            //         break;

            //     case Sentiment.Happy:
            //         if (TriggerCount > 3)
            //         {
            //             TriggerCount = 0;
            //             PreferredSentiment = Sentiment.Sad;
            //             StageSwitcher.Instance.SwitchToStage(2);
            //         }

            //         break;

            //     case Sentiment.Sad:
            //         if (TriggerCount > 3)
            //         {
            //             TriggerCount = 0;
            //             PreferredSentiment = Sentiment.Wish;
            //             StageSwitcher.Instance.SwitchToStage(3);
            //         }

            //         break;

            //     case Sentiment.Wish:
            //         if (TriggerCount > 5)
            //         {
            //             TriggerCount = 0;
            //             PreferredSentiment = Sentiment.Neutral;
            //             StageSwitcher.Instance.SwitchToStage(1);
            //         }

            //         break;
            // }
        }
    }
}
