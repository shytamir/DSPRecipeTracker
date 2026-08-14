using System.Collections.Generic;

namespace UnityEngine.EventSystems
{
    public abstract class UIBehaviour : UnityEngine.MonoBehaviour
    {
        protected UIBehaviour()
        {
        }
    }

    public class BaseEventData
    {
        protected BaseEventData()
        {
        }
    }

    public class PointerEventData : BaseEventData
    {
        private PointerEventData()
        {
        }
        public enum InputButton
        {
            Left,
            Right,
            Middle
        }

        public InputButton button
        {
            get { return InputButton.Left; }
        }
    }

    public enum EventTriggerType
    {
        PointerEnter,
        PointerExit,
        PointerDown
    }

    public class EventTrigger : UnityEngine.MonoBehaviour
    {
        private EventTrigger()
        {
        }

        public class TriggerEvent : UnityEngine.Events.UnityEvent<BaseEventData>
        {
            private TriggerEvent()
            {
            }
        }

        public class Entry
        {
            private Entry()
            {
            }

            public EventTriggerType eventID;
            public TriggerEvent callback;
        }

        public List<Entry> triggers
        {
            get { return null; }
        }
    }
}

namespace UnityEngine.UI
{
    public abstract class Graphic : UnityEngine.EventSystems.UIBehaviour
    {
        protected Graphic()
        {
        }

        public virtual bool raycastTarget
        {
            set { }
        }
    }

    public abstract class MaskableGraphic : Graphic
    {
        protected MaskableGraphic()
        {
        }
    }

    public class Image : MaskableGraphic
    {
        protected Image()
        {
        }

        public UnityEngine.Sprite sprite
        {
            set { }
        }
    }
}
