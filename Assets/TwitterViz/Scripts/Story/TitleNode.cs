using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TitleNode : StoryNode
{
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
        Debug.Log("trigger enter");
        GuidingLight guidingLight = GetComponentInChildren<GuidingLight>();
        guidingLight.gameObject.SetActive(false);

        GetComponent<Animator>().enabled = true;
        GetComponentInChildren<TextMeshPro>(true).gameObject.SetActive(true);
    }
}
