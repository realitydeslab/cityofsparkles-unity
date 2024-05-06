using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace CityOfSparkles.VisionOS
{
    public class DebugUI_MockPinch : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public UnityEvent OnPointerDownAction;

        public UnityEvent OnPointerUpAction;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPointerDownAction?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnPointerUpAction?.Invoke();
        }
    }
}
