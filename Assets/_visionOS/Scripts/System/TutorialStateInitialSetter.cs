using UnityEngine;

namespace CityOfSparkles.VisionOS
{
    public class TutorialStateInitialSetter : MonoBehaviour
    {
        private bool m_HasSet = false;

        private void Update()
        {
            if (!m_HasSet)
            {
                TutorialStateManager.Instance.State = TutorialState.InitialRedDot;
                m_HasSet = true;
            }
        }
    }
}
