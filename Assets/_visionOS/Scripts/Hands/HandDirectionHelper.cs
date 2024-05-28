using UnityEngine;

namespace CityOfSparkles.VisionOS
{
    [RequireComponent(typeof(EditorGameObjectDestroyer))]
    public class HandDirectionHelper : MonoBehaviour
    {
        [SerializeField] private Transform m_Target;

        private void Update()
        {
            if (m_Target != null)
            {
                Vector3 direction = (m_Target.position - transform.position).normalized;
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}