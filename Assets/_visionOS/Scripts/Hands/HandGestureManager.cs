using UnityEngine;
using UnityEngine.XR.Hands;
using System.Collections.Generic;
using UnityEngine.Events;

namespace CityOfSparkles.VisionOS
{
    public enum HandGesture
    {
        NotTracked = 0,
        None = 1,
        Pinching = 2
    }

    public class HandGestureManager : MonoBehaviour
    {
        public Dictionary<Handedness, HandGesture> HandGestures => m_HandGestures;

        private readonly Dictionary<Handedness, HandGesture> m_HandGestures = new();

        private const float PINCH_DIST = 0.01f;

        public UnityEvent<Handedness, HandGesture, HandGesture> OnHandGestureChanged;

        private void Start()
        {
            m_HandGestures.Add(Handedness.Left, HandGesture.NotTracked);
            m_HandGestures.Add(Handedness.Right, HandGesture.NotTracked);
        }

        public void OnTrackingAcquired_Left()
        {
            OnTrackingAcquired(Handedness.Left);
        }

        public void OnTrackingAcquired_Right()
        {
            OnTrackingAcquired(Handedness.Right);
        }

        private void OnTrackingAcquired(Handedness handedness)
        {
            OnHandGestureChangedInternal(handedness, HandGesture.None);
        }

        public void OnTrackingLost_Left()
        {
            OnTrackingLost(Handedness.Left);
        }

        public void OnTrackingLost_Right()
        {
            OnTrackingLost(Handedness.Right);
        }

        private void OnTrackingLost(Handedness handedness)
        {
            OnHandGestureChangedInternal(handedness, HandGesture.NotTracked);
        }

        public void OnJointsUpdated(XRHandJointsUpdatedEventArgs args)
        {
            OnUpdatedHand(args.hand);
        }

        private void OnUpdatedHand(XRHand hand)
        {
            if (ValidateHandGesture_Pinching(hand))
            {
                OnHandGestureChangedInternal(hand.handedness, HandGesture.Pinching);
                return;
            }

            OnHandGestureChangedInternal(hand.handedness, HandGesture.None);
        }

        private void OnHandGestureChangedInternal(Handedness handedness, HandGesture handGesture)
        {
            if (m_HandGestures[handedness] == handGesture)
                return;

            HandGesture prevGesture = m_HandGestures[handedness];
            m_HandGestures[handedness] = handGesture;
            OnHandGestureChanged?.Invoke(handedness, prevGesture, handGesture);
        }

        private bool ValidateHandGesture_Pinching(XRHand hand)
        {
            if (hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out var thumbTipPose) && hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out var indexTipPose))
            {
                return Vector3.Distance(thumbTipPose.position, indexTipPose.position) < PINCH_DIST;
            }

            return false;
        }

        // For debug purpose
        public void SetHandGesture(Handedness handedness, HandGesture handGesture)
        {
            m_HandGestures[handedness] = handGesture;
        }
    }
}
