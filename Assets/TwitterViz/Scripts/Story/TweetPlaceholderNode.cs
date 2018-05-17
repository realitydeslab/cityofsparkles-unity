using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweetPlaceholder : StoryNode
{
    public TweetPlaceholder[] Next;
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
}
