using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Raccoon.UiInput
{

    public class UIVirtualButtonRotateDrop : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [System.Serializable]
        public class BoolEvent : UnityEvent<bool, Vector2> { }
        [System.Serializable]
        public class Event : UnityEvent { }

        [Header("Output")]
        public BoolEvent buttonStateOutputEvent;
        public Event buttonClickOutputEvent;

        bool isDrag;
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            OutputButtonStateValue(true, eventData.position);

            isDrag = false;
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            OutputButtonStateValue(false, eventData.position);

            if(!isDrag) OutputButtonClickEvent();

            isDrag = false;
        }
        public void OnDrag(PointerEventData eventData)
        {
            isDrag = true;  
        }


        protected void OutputButtonStateValue(bool buttonState , Vector2 eventPos)
        {
            buttonStateOutputEvent.Invoke(buttonState, eventPos);
        }

        protected void OutputButtonClickEvent()
        {
            buttonClickOutputEvent.Invoke();
        }

    }
}