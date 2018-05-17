using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpawnSource
{
    Vector3? GetPosition(TwitterDatabase.DBTweet data);

    void OnTweetSpawned(TweetComponent tweet);
    void OnTweetTriggered(TweetComponent tweet);
    void OnTweetRevealed(TweetComponent tweet);
}

public class StoryNode : MonoBehaviour
{
    public bool EnableOnAwake;

    void Awake()
    {
        if (!EnableOnAwake)
        {
            enabled = false;
        }
    }

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
