using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace DSPRecipeTracker
{
    internal sealed class UnityTrackerPanelDragAdapter : ITrackerPanelDragAdapter
    {
        private readonly UnityTrackerPanelAdapter panel;
        private readonly Canvas overlayCanvas;
        private EventTrigger trigger;
        private EventTrigger.Entry dragEntry;
        private EventTrigger.Entry endDragEntry;
        private UnityAction<BaseEventData> dragListener;
        private UnityAction<BaseEventData> endDragListener;
        private Action<DragDelta> drag;
        private Action dragCompleted;
        private bool released;

        public UnityTrackerPanelDragAdapter(UnityTrackerPanelAdapter panel, Canvas overlayCanvas)
        {
            this.panel = panel;
            this.overlayCanvas = overlayCanvas;
        }

        public bool TryAttach(Action<DragDelta> drag, Action dragCompleted)
        {
            var panelTransform = panel?.PanelTransform;
            if (released ||
                !ReferenceEquals(trigger, null) ||
                ReferenceEquals(panelTransform, null) ||
                ReferenceEquals(overlayCanvas, null) ||
                drag == null ||
                dragCompleted == null)
            {
                return false;
            }

            this.drag = drag;
            this.dragCompleted = dragCompleted;
            trigger = (EventTrigger)panelTransform.gameObject.AddComponent(typeof(EventTrigger));
            if (ReferenceEquals(trigger, null) || ReferenceEquals(trigger.triggers, null))
            {
                return false;
            }

            dragListener = OnDrag;
            endDragListener = OnEndDrag;
            dragEntry = CreateEntry(EventTriggerType.Drag, dragListener);
            endDragEntry = CreateEntry(EventTriggerType.EndDrag, endDragListener);
            trigger.triggers.Add(dragEntry);
            trigger.triggers.Add(endDragEntry);
            return true;
        }

        public bool TryReadScaleFactor(out float scaleFactor)
        {
            scaleFactor = 0f;
            if (released || ReferenceEquals(overlayCanvas, null))
            {
                return false;
            }

            scaleFactor = overlayCanvas.scaleFactor;
            return true;
        }

        public bool TryReadParentSize(out float width, out float height)
        {
            width = 0f;
            height = 0f;
            var panelTransform = panel?.PanelTransform;
            var parent = ReferenceEquals(panelTransform, null)
                ? null
                : panelTransform.parent as RectTransform;
            if (released || ReferenceEquals(parent, null))
            {
                return false;
            }

            var rectangle = parent.rect;
            width = rectangle.width;
            height = rectangle.height;
            return true;
        }

        public void Release()
        {
            if (released)
            {
                return;
            }

            released = true;
            if (!ReferenceEquals(dragEntry, null) &&
                !ReferenceEquals(dragEntry.callback, null) &&
                !ReferenceEquals(dragListener, null))
            {
                dragEntry.callback.RemoveListener(dragListener);
            }

            if (!ReferenceEquals(endDragEntry, null) &&
                !ReferenceEquals(endDragEntry.callback, null) &&
                !ReferenceEquals(endDragListener, null))
            {
                endDragEntry.callback.RemoveListener(endDragListener);
            }

            if (!ReferenceEquals(trigger, null) && !ReferenceEquals(trigger.triggers, null))
            {
                trigger.triggers.Remove(dragEntry);
                trigger.triggers.Remove(endDragEntry);
            }

            var ownedTrigger = trigger;
            trigger = null;
            dragEntry = null;
            endDragEntry = null;
            dragListener = null;
            endDragListener = null;
            drag = null;
            dragCompleted = null;
            if (!ReferenceEquals(ownedTrigger, null))
            {
                UnityEngine.Object.Destroy(ownedTrigger);
            }
        }

        private static EventTrigger.Entry CreateEntry(
            EventTriggerType eventType,
            UnityAction<BaseEventData> listener)
        {
            var callback = new EventTrigger.TriggerEvent();
            callback.AddListener(listener);
            return new EventTrigger.Entry
            {
                eventID = eventType,
                callback = callback
            };
        }

        private void OnDrag(BaseEventData eventData)
        {
            if (released || !(eventData is PointerEventData pointerEvent))
            {
                return;
            }

            var delta = pointerEvent.delta;
            drag?.Invoke(new DragDelta(delta.x, -delta.y));
        }

        private void OnEndDrag(BaseEventData eventData)
        {
            if (!released && eventData is PointerEventData)
            {
                dragCompleted?.Invoke();
            }
        }
    }
}
