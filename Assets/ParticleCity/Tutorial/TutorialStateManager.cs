using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
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

    [Header("Auto")] 
    public TutorialState State;

    public bool FixedDirectionFlyPassed = false;

    [CanBeNull] public StoryNode Source;
    public float MaxAngleToSource = 10;

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
                if (StageSwitcher.Instance.CurrentStage != Stage.InitialDark && StageSwitcher.Instance.CurrentStage != Stage.Intro)
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

    public void SetStateByStoryNode(TutorialState state, StoryNode source)
    {
        State = state;
        Source = source;
    }

    public void InitialRedDotCloseEnough()
    {
        State = TutorialState.ReachOutHand;
    }

    public void RedDotReached()
    {
        State = IsInInitialTutorialSteps ? TutorialState.TriggerByGrab : TutorialState.TriggerByGrabLaterHint;
    }

    public void InitialRedDotMissed()
    {
        State = TutorialState.InitialRedDotMissed;
        StartCoroutine(SwitchToStateWithDelay(FixedDirectionFlyPassed ? TutorialState.FlyWithRotate : TutorialState.InitialRedDot, 4));
    }

    public void SecondRedDotReached()
    {
        State = TutorialState.Idle;
    }

    public void TwitterTriggered()
    {
        State = TutorialState.Idle;
        FixedDirectionFlyPassed = true;
    }

    public bool IsInInitialTutorialSteps
    {
        get
        {
            return State == TutorialState.InitialRedDot ||
                   State == TutorialState.InitialRedDotMissed ||
                   State == TutorialState.ReachOutHand ||
                   State == TutorialState.TriggerByGrab ||
                   State == TutorialState.FlyWithRotate;
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
    Invalid = 0,
    Idle,
    InitialRedDot,
    ReachOutHand,
    TriggerByGrab,
    InitialRedDotMissed,
    TriggerByGrabLaterHint,
    FlyWithRotate,
}
