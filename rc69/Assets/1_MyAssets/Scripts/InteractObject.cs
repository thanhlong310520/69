using UnityEngine;
using UnityEngine.UI;

namespace Raccoon.Controller
{
    public abstract class InteractObject : MonoBehaviour
    {
        public string id;

        protected Outline highlight;

        protected virtual void LoadComponent()
        {
            if (highlight == null)
                highlight = gameObject.GetComponentInChildren<Outline>();
        }

        public void SetHighlightObject(bool enable)
        {
            if (highlight == null) return;
            highlight.enabled = enable;
        }

        protected void ReinitializeHighlight()
        {
            LoadComponent();
            if (highlight == null) return;
            //highlight.Reinitialize();
        }

    }
}