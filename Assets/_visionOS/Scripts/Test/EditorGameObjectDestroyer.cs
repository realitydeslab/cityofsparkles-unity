using UnityEngine;

namespace CityOfSparkles.VisionOS
{
    public class EditorGameObjectDestroyer : MonoBehaviour
    {
        private void Awake()
        {
#if !UNITY_EDITOR
            Destroy(gameObject);
#endif
        }
    }
}