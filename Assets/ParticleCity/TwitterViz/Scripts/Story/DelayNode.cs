using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayNode : StoryNode 
{
    public override void Start()
    {
        GotoNext();
        Destroy(gameObject);
    }
}
