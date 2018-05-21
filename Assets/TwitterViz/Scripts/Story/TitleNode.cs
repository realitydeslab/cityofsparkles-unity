using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TitleNode : StoryNode
{
    private bool triggered = false; 

    public override void Awake()
    {
        base.Awake();
        GetComponentInChildren<TextMeshPro>().gameObject.SetActive(false);
    }

    public void OnFinished()
    {
        GotoNext();
        Destroy(gameObject);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (triggered)
        {
            return;
        }

        Debug.Log("trigger enter");
        GuidingLight guidingLight = GetComponentInChildren<GuidingLight>();
        guidingLight.gameObject.SetActive(false);

        GetComponent<Animator>().enabled = true;
        GetComponentInChildren<TextMeshPro>(true).gameObject.SetActive(true);

        triggered = true;
    }
}
