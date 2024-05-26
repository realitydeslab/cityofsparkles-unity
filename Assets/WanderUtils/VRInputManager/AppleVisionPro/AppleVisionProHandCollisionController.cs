using UnityEngine;
using UnityEngine.XR.Hands;

namespace ParticleCities
{
    public class AppleVisionProHandCollisionController : MonoBehaviour
    {
        [SerializeField] private Handedness m_Handedness;

        public Handedness Handedness => m_Handedness;
    }
}
