using System.Collections;
using System.Collections.Generic;
using ParticleCities;
using UnityEngine;

public class StoryNode : MonoBehaviour
{
    public bool EnabledOnStart = false;
    private bool isFirstAwake = true;

    public List<StoryNode> Next;
    public Stage SwitchToStage = Stage.Invalid;

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

    public virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 30);
    }

    public virtual void OnDrawGizmosSelected()
    {
        if (Next == null)
        {
            return;
        }

        for (int i = 0; i < Next.Count; i++)
        {
            if (Next[i].Equals(null))
            {
                continue;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, Next[i].transform.position);
        }
    }

    protected void GotoNext()
    {
        for (int i = 0; i < Next.Count; i++)
        {
            Next[i].gameObject.SetActive(true);
        }

        if (SwitchToStage != Stage.Invalid)
        {
            StageSwitcher.Instance.SwitchToStage(SwitchToStage);
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

