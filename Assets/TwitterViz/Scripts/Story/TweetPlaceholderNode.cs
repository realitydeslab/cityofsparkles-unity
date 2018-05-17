using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweetPlaceholderNode : StoryNode, ISpawnSource
{
    public TweetPlaceholderNode[] Next;
    public int SwitchToStage = -1;

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

    public void OnTweetRevealed(TweetComponent obj)
    {
        Destroy(gameObject);
    }

    public void OnTweetSpawned(TweetComponent tweet)
    {
    }

    public void OnTweetTriggered(TweetComponent tweet)
    {

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

    public Vector3? GetPosition(TwitterDatabase.DBTweet data)
    {
        return transform.position;
    }
}
