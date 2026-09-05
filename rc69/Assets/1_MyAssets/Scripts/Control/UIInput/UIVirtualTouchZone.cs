using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;


namespace Raccoon.UiInput
{

    public class UIVirtualTouchZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [System.Serializable]
        public class Event : UnityEvent<Vector2> { }


        [Header("Output")]
        public Event touchZoneOutputDragEvent;
        public Event touchZoneOutputClickEvent;

        bool isDrag;




        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isDrag)
            {
                OutputPointerClickEventValue(eventData.position);
                isDrag = false;
            }

        }

        void OutputPointerDragEventValue(Vector2 pointerPosition)
        {
            touchZoneOutputDragEvent.Invoke(pointerPosition);
        }
        void OutputPointerClickEventValue(Vector2 pointerPosition)
        {
            touchZoneOutputClickEvent.Invoke(pointerPosition);
        }




        Vector2 lastPos;

        bool hasMoved;
        Vector2 velocity;
        public float stopThreshold = 5f;
        public void OnPointerDown(PointerEventData eventData)
        {
            lastPos = eventData.position;
            hasMoved = false;
            isDrag = false;
            velocity = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            isDrag = true;
            Vector2 delta = eventData.position - lastPos;
            float deltaTime = Time.unscaledDeltaTime;

            // Tính tốc độ kéo (px/s)
            velocity = delta / Mathf.Max(deltaTime, 0.0001f);
            hasMoved = velocity.magnitude > stopThreshold;

            // Chỉ cập nhật góc mục tiêu khi đang vuốt
            if (hasMoved)
            {
                velocity = delta;
            }
            else
            {
                velocity = Vector2.zero;
            }

            lastPos = eventData.position;
        }



        private void Update()
        {
            velocity.Normalize();
            OutputPointerDragEventValue(velocity);
            velocity = Vector2.zero;
        }

    }
}
