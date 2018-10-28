using System.Collections;
using System.Collections.Generic;
using ParticleCities;
using UnityEngine;

public class TweetPlaceholderNode : SpawnSourceNode
{
    public string QueryTag;
    public float TargetCityIntensity = -1;
    public float IntensityLerpRatio = 0.01f;

    public float MixInRatio = 0;
    public string AkEventOnSpawn;
    public string AkEventOnReveal = "Play_TweetRevealCommon";

    [Tooltip("Trigger the next story node without spawning tweets.")]
    public bool IsStoryTrigger;

    public bool musicSync = false;
    public override bool MusicSync
    {
        get { return musicSync; }
    }

    [Header("Animation")] 
    public bool Trigger;

    [Header("Tutorial")] 
    public TutorialState TutorialStateOnSpawn;

    public TweetComponent SpawnedTweet { get; private set; }

    private TwitterDatabase.DBTweet tweet;
    private bool spawned;

    public override void Update()
    {
        base.Update();

        if (IsStoryTrigger)
        {
            if (!spawned)
            {
                TwitterManager.Instance.Enqueue(new[]
                {
                    new TwitterManager.SpawnRequest
                    {
                        Data = TwitterDatabase.DBTweet.EmptyPlaceholder(),
                        Source = this
                    }
                });
                spawned = true;
            }
        }
        else
        {
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
                TwitterManager.Instance.Enqueue(new[]
                {
                    new TwitterManager.SpawnRequest
                    {
                        Data = tweet,
                        Source = this
                    }
                });
                spawned = true;
            }

            if (Trigger)
            {
                Trigger = false;
                if (SpawnedTweet != null)
                {
                    SpawnedTweet.Trigger = true;
                }
            }
        }
    }

    public override void OnTweetSpawned(TweetComponent tweet)
    {
        base.OnTweetSpawned(tweet);
        SpawnedTweet = tweet;

        if (!string.IsNullOrEmpty(AkEventOnSpawn))
        {
            AkSoundEngine.PostEvent(AkEventOnSpawn, tweet.gameObject);
        }

        // Decorate tweet component
        if (IsStoryTrigger)
        {
            tweet.NodeRole = TweetComponent.NodeRoleType.StoryTrigger;
        }

        if (!string.IsNullOrEmpty(AkEventOnReveal))
        {
            tweet.AkEventOnReveal = AkEventOnReveal;
        }

        if (TutorialStateOnSpawn != TutorialState.Invalid)
        {
            TutorialStateManager.Instance.SetStateByStoryNode(TutorialStateOnSpawn, this);
        }
    }

    public override void GotoNext()
    {
        if (HasNext())
        {
            TwitterManager.Instance.ClearAll();
        }

        if (MixInRatio > 0)
        {
            AkSoundEngine.SetRTPCValue("MixInRatio", MixInRatio);
        }

        if (SwitchToStage != Stage.Invalid)
        {
            AkSoundEngine.SetRTPCValue("MixInRatio", 0);
        }

        base.GotoNext();

        if (TargetCityIntensity >= 0)
        {
            ParticleCity.Current.Animator.LerpToIntensity(TargetCityIntensity, IntensityLerpRatio);
        }

        Destroy(gameObject);
    }

    public override void OnTweetRevealed(TweetComponent obj)
    {
        GotoNext();
    }

    public override void OnTweetDestroy(TweetComponent tweet)
    {
        Debug.Log("Tweet destroy");
        Destroy(gameObject);
    }

    public override Vector3? GetPosition(TwitterDatabase.DBTweet data)
    {
        return transform.position;
    }

    public override bool IsTriggered
    {
        get
        {
            return SpawnedTweet != null && SpawnedTweet.IsTriggered;
        }
    }
}
