using UnityEngine;
using UnityEngine.XR.Hands;

namespace CityOfSparkles.VisionOS
{
    public class DebugUI : MonoBehaviour
    {
        [SerializeField] private HandGestureManager m_HandGestureManager;

        public void OnStartedPinching_Left()
        {
            m_HandGestureManager.SetHandGesture(Handedness.Left, HandGesture.Pinching);
        }

        public void OnStoppedPinching_Left()
        {
            m_HandGestureManager.SetHandGesture(Handedness.Left, HandGesture.None);
        }

        public void OnStartedPinching_Right()
        {
            m_HandGestureManager.SetHandGesture(Handedness.Right, HandGesture.Pinching);
        }

        public void OnStoppedPinching_Right()
        {
            m_HandGestureManager.SetHandGesture(Handedness.Right, HandGesture.None);
        }
    }
}
