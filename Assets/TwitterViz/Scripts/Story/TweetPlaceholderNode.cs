using System.Collections;
using System.Collections.Generic;
using ParticleCities;
using UnityEngine;

public class TweetPlaceholderNode : SpawnSourceNode
{
    public StoryNode[] Next;
    public Stage SwitchToStage = Stage.Invalid;
    public string QueryTag;
    public float TargetCityIntensity = -1;

    public float MixInRatio = 0;
    public string AkEventOnSpawn; 

    private TwitterDatabase.DBTweet tweet;
    private bool spawned;

    public override void Update()
    {
        base.Update();

        if (tweet == null)
        {
            IList<TwitterDatabase.DBTweet> tweets = TwitterManager.Instance.Database.QueryForTags(QueryTag, 1);
            if (tweets.Count == 0)
            {
                Debug.LogWarning("Cannot find tweet for placeholder. ", this);
            }
            else
            {
                tweet = tweets[0];
            }
        }

        if (tweet != null && !spawned)
        {
            TwitterManager.Instance.Enqueue(new [] {new TwitterManager.SpawnRequest
            {
                Data = tweet,
                Source = this
            }});
            spawned = true;
        }
    }

    public override void OnTweetSpawned(TweetComponent tweet)
    {
        base.OnTweetSpawned(tweet);

        if (!string.IsNullOrEmpty(AkEventOnSpawn))
        {
            AkSoundEngine.PostEvent(AkEventOnSpawn, tweet.gameObject);
        }
    }

    public override void OnTweetRevealed(TweetComponent obj)
    {
        TwitterManager.Instance.ClearAll();
        for (int i = 0; i < Next.Length; i++)
        {
            Next[i].gameObject.SetActive(true);
        }

        if (MixInRatio > 0)
        {
            AkSoundEngine.SetRTPCValue("MixInRatio", MixInRatio);
        }

        if (SwitchToStage != Stage.Invalid)
        {
            AkSoundEngine.SetRTPCValue("MixInRatio", 0);
            StageSwitcher.Instance.SwitchToStage(SwitchToStage);
        }

        if (TargetCityIntensity >= 0)
        {
            ParticleCity.Current.Animator.LerpToIntensity(TargetCityIntensity, 0.01f);
        }

        Destroy(gameObject);
    }

    public override void OnTweetDestroy(TweetComponent tweet)
    {
        Debug.Log("Tweet destroy");
        Destroy(gameObject);
    }

    public override IList<StoryNode> GetNextNodes()
    {
        return Next;
    }

    public override Vector3? GetPosition(TwitterDatabase.DBTweet data)
    {
        return transform.position;
    }
}
