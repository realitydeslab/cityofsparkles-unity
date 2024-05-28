using UnityEngine;

namespace CityOfSparkles.VisionOS
{
    public class HandPoseIndicator : MonoBehaviour
    {
        [SerializeField] private Transform m_LeftHand;

        [SerializeField] private Transform m_RightHand;

        [SerializeField] private Transform m_LeftThumbMetacarpal;

        [SerializeField] private Transform m_LeftIndexProximal;

        [SerializeField] private Transform m_RightThumbMetacarpal;

        [SerializeField] private Transform m_RightIndexProximal;

        private void Update()
        {
            Vector3 leftDirection = (m_LeftIndexProximal.position - m_LeftThumbMetacarpal.position).normalized;
            m_LeftHand.position = m_LeftIndexProximal.position;
#if !UNITY_EDITOR
            m_LeftHand.rotation = Quaternion.LookRotation(leftDirection);
#endif

            Vector3 rightDirection = (m_RightIndexProximal.position - m_RightThumbMetacarpal.position).normalized;
            m_RightHand.position = m_RightIndexProximal.position;
#if !UNITY_EDITOR
            m_RightHand.rotation = Quaternion.LookRotation(rightDirection);
#endif
        }
    }
}
