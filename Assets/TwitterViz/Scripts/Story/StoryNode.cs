using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryNode : MonoBehaviour
{
    public bool EnabledOnStart = false;
    private bool isFirstAwake = true;

    public virtual void Awake()
    {
        if (isFirstAwake && !EnabledOnStart)
        {
            isFirstAwake = false;
            gameObject.SetActive(false);
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

    public virtual IList<StoryNode> GetNextNodes()
    {
        return null;
    }

    public virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 30);
    }

    public virtual void OnDrawGizmosSelected()
    {
        IList<StoryNode> nodes = GetNextNodes();
        if (nodes == null)
        {
            return;
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].Equals(null))
            {
                continue;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, nodes[i].transform.position);
        }
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

