using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryNode : MonoBehaviour
{
	public virtual void Start() {
		
	}

    public virtual void OnEnable()
    {

    }

    public virtual void OnDisable()
    {

    }
	
	public virtual void Update() 
	{
		
	}

    public virtual void OnDestroy()
    {

    }
}

public abstract class SpawnSourceNode : StoryNode
{
    public virtual Vector3? GetPosition(TwitterDatabase.DBTweet data)
    {
        return null;
    }

    public virtual void OnTweetSpawned(TweetComponent tweet)
    {

    }

    public virtual void OnTweetTriggered(TweetComponent tweet)
    {

    }

    public virtual void OnTweetRevealed(TweetComponent tweet)
    {

    }

    public virtual void OnTweetDestroy(TweetComponent tweet)
    {

    }
}

