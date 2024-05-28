using UnityEngine;

namespace CityOfSparkles.VisionOS
{
    public class HandDirectionHelper : MonoBehaviour
    {
        [SerializeField] private Transform m_Target;

        private void Update()
        {
#if UNITY_EDITOR
            if (m_Target != null)
            {
                Vector3 direction = (m_Target.position - transform.position).normalized;
                transform.rotation = Quaternion.LookRotation(direction);
            }
#endif
        }
    }
}