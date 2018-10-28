using System.Collections;
using System.Collections.Generic;
using ParticleCities;
using UnityEngine;

public class TutorialStateManager : MonoBehaviour
{
    private static TutorialStateManager instance;
    public static TutorialStateManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TutorialStateManager>();
            }

            return instance;
        }
    }

    [Header("Debug")] 
    public TutorialState State;

    void Start()
    {
        State = TutorialState.InitialRedDot;
    }

    void Update()
    {
        switch (State)
        {
            case TutorialState.InitialRedDot:
            case TutorialState.TriggerByGrab:
                if (StageSwitcher.Instance.CurrentStage == Stage.Intro)
                {
                    State = TutorialState.FlyWithRotate;
                }
                else if (StageSwitcher.Instance.CurrentStage != Stage.InitialDark)
                {
                    State = TutorialState.Idle;
                }
                break;

            case TutorialState.FlyWithRotate:
                if (StageSwitcher.Instance.CurrentStage != Stage.Intro)
                {
                    State = TutorialState.Idle;
                }
                break;

            default:
                break;
        }
    }

    public void InitialRedDotCloseEnough()
    {
        State = TutorialState.ReachOutHand;
    }

    public void RedDotReached()
    {
        State = IsInInitialDarkSteps ? TutorialState.TriggerByGrab : TutorialState.TriggerByGrabLaterHint;
    }

    public void InitialRedDotMissed()
    {
        State = TutorialState.InitialRedDotMissed;
        StartCoroutine(SwitchToStateWithDelay(TutorialState.InitialRedDot, 4));
    }

    public void InitialRedDotTriggered()
    {
        State = TutorialState.FlyWithRotate;
    }

    public void SecondRedDotReached()
    {
        State = TutorialState.Idle;
    }

    public void TwitterTriggered()
    {
        State = TutorialState.Idle;
    }

    public bool IsInInitialDarkSteps
    {
        get
        {
            return State == TutorialState.InitialRedDot ||
                   State == TutorialState.InitialRedDotMissed ||
                   State == TutorialState.ReachOutHand ||
                   State == TutorialState.TriggerByGrab;
        }
    }

    private IEnumerator SwitchToStateWithDelay(TutorialState nextState, float delay)
    {
        TutorialState expectedCurrentState = State; 
        yield return new WaitForSeconds(delay);
        if (State == expectedCurrentState)
        {
            State = nextState;
        }
    }
}

public enum TutorialState
{
    Idle = 0,
    InitialRedDot,
    ReachOutHand,
    TriggerByGrab,
    InitialRedDotMissed,
    TriggerByGrabLaterHint,
    FlyWithRotate,
}
