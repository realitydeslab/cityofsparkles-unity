using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using TwitterViz.DataModels;
using UnityEngine;
using WanderUtils;

[RequireComponent(typeof(Animator))]
public class TweetCanvas : MonoBehaviour
{
    public TwitterDatabase.DBTweet Tweet;

    [Header("UI Components")]
    public TextMeshProUGUI DisplayNameUI;
    public TextMeshProUGUI UserNameUI;
    public TextMeshProUGUI ContentUI;
    public TextMeshProUGUI DateUI;
    public GameObject[] MotionEffectors; 

    [Header("Word holding for random tweets")]
    public float HoldingTimeOnGaze = 2;
    public float HoldingTimeNoGaze = 5;

    private float holdingTime;
    private float gazeTime;
    private bool fadeInFinished;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        DisplayNameUI.text = strip(Tweet.full_username);
        UserNameUI.text = "@" + Tweet.username;
        ContentUI.text = strip(Tweet.full_text);

        DateTime createdAt = DateTime.Parse(Tweet.created_at);
        DateUI.text = createdAt.ToString("hh:mm tt - d MMM yyyy");

        for (int i = 0; i < MotionEffectors.Length; i++)
        {
            ParticleCity.Current.AddActiveGameObject(MotionEffectors[i]);
        }

    }

    void Update()
    {
        if (!fadeInFinished)
        {
            return;
        }

        holdingTime += Time.deltaTime;
        if (holdingTime > HoldingTimeNoGaze)
        {
            animator.SetTrigger("FadeOut");
        }
        else
        {
            float angle = Vector3.Angle(InputManager.Instance.CenterCamera.transform.forward, transform.position - InputManager.Instance.CenterCamera.transform.position);
            if (angle < 45)
            {
                gazeTime += Time.deltaTime;
            }
            else
            {
                gazeTime = 0;
            }

            if (gazeTime > HoldingTimeOnGaze)
            {
                animator.SetTrigger("FadeOut");
            }
        }
    }

    public void OnDestroy()
    {
    }

    public void OnFadeInFinished()
    {
        fadeInFinished = true;
        for (int i = 0; i < MotionEffectors.Length; i++)
        {
            ParticleCity.Current.RemoveActiveGameObject(MotionEffectors[i]);
        }
    }

    public void OnFadeOutFinished()
    {
        Destroy(gameObject);
    }

    private string strip(string raw)
    {
        if (raw == null)
        {
            return null;
        }

        string stripped = Regex.Replace(raw, @"[^\u0000-\u007F]+", string.Empty);
        // stripped = Regex.Replace(stripped, @",(\S)", @", $1");
        return stripped;
    }
}
