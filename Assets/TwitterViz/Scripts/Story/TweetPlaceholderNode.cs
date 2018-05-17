﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweetPlaceholderNode : SpawnSourceNode
{
    public StoryNode[] Next;
    public int SwitchToStage = -1;

    private TwitterDatabase.DBTweet tweet;
    private bool spawned;

    public TwitterDatabase.DBTweet QueryData(TwitterDatabase database)
    {
        TwitterDatabase.DBTweet result = database.QueryOne();
        if (result == null)
        {
            Debug.LogWarning("Cannot find tweet for placeholder. ", this);
        }

        return result;
    }

    public override void OnEnable()
    {
        base.OnEnable();

    }

    public override void Update()
    {
        base.Update();

        if (tweet == null)
        {
            tweet = TwitterManager.Instance.Database.QueryOne();
            if (tweet == null)
            {
                Debug.LogWarning("Cannot find tweet for placeholder. ", this);
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

    public override void OnTweetRevealed(TweetComponent obj)
    {
        TwitterManager.Instance.ClearAll();
        for (int i = 0; i < Next.Length; i++)
        {
            Next[i].enabled = true; 
        }

        if (SwitchToStage >= 0)
        {
            StageSwitcher.Instance.SwitchToStage(SwitchToStage);
        }

        Destroy(gameObject);
    }

    public override void OnTweetDestroy(TweetComponent tweet)
    {
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        for (int i = 0; i < Next.Length; i++)
        {
            if (Next[i] == null)
            {
                continue;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, Next[i].transform.position);
        }
    }

    public override Vector3? GetPosition(TwitterDatabase.DBTweet data)
    {
        return transform.position;
    }
}
