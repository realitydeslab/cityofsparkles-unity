using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwitterTest : MonoBehaviour
{
    [SerializeField] private TwitterDatabase m_TwitterDatabase;

    private void Start()
    {
        //TwitterDatabase.DBTweet tweet = m_TwitterDatabase.QueryForRandomTweet();
        //TwitterDatabase.DBTweet tweet = m_TwitterDatabase.QueryOne();
        var tweets = m_TwitterDatabase.QueryForTags("nice", 1);

        Debug.Log("tweets.Count: " + tweets.Count);
        Debug.Log("tweets[0]: " + tweets[0]);
    }
}
