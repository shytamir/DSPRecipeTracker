using System;
using System.Reflection;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace DSPRecipeTracker
{
    internal sealed class UnityReplicatorPinInputAdapter : IReplicatorPinInputAdapter
    {
        internal const string EventFieldName = "evtRecipe";
        internal const string RecipeArrayFieldName = "recipeProtoArray";
        internal const string RecipeIndexFieldName = "mouseRecipeIndex";

        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        private readonly UIReplicatorWindow window;
        private readonly FieldInfo eventField;
        private readonly FieldInfo recipeArrayField;
        private readonly FieldInfo recipeIndexField;
        private UnityEvent<BaseEventData> pointerDownEvent;
        private UnityAction<BaseEventData> listener;
        private bool released;

        public UnityReplicatorPinInputAdapter(UIReplicatorWindow window)
        {
            this.window = window;
            var windowType = typeof(UIReplicatorWindow);
            eventField = windowType.GetField(EventFieldName, PrivateInstance);
            recipeArrayField = windowType.GetField(RecipeArrayFieldName, PrivateInstance);
            recipeIndexField = windowType.GetField(RecipeIndexFieldName, PrivateInstance);
        }

        public bool TryAttach(Action<ReplicatorPointerButton> pointerDown)
        {
            if (released || ReferenceEquals(window, null) || pointerDown == null ||
                eventField == null || recipeArrayField == null || recipeIndexField == null)
            {
                return false;
            }

            var trigger = eventField.GetValue(window) as EventTrigger;
            if (ReferenceEquals(trigger, null) || trigger.triggers == null)
            {
                return false;
            }

            foreach (var entry in trigger.triggers)
            {
                if (entry != null && entry.eventID == EventTriggerType.PointerDown && entry.callback != null)
                {
                    pointerDownEvent = entry.callback;
                    listener = eventData => pointerDown(MapButton(eventData));
                    pointerDownEvent.AddListener(listener);
                    return true;
                }
            }

            return false;
        }

        public bool TryGetCurrentRecipe(out int gridIndex, out int recipeId)
        {
            gridIndex = -1;
            recipeId = 0;
            if (released || ReferenceEquals(window, null) || recipeArrayField == null || recipeIndexField == null)
            {
                return false;
            }

            var recipes = recipeArrayField.GetValue(window) as RecipeProto[];
            var indexValue = recipeIndexField.GetValue(window);
            if (recipes == null || !(indexValue is int index) || index < 0 || index >= recipes.Length)
            {
                return false;
            }

            var recipe = recipes[index];
            if (recipe == null)
            {
                return false;
            }

            recipeId = recipe.ID;
            gridIndex = index;
            return true;
        }

        public void Release()
        {
            if (released)
            {
                return;
            }

            released = true;
            if (pointerDownEvent != null && listener != null)
            {
                pointerDownEvent.RemoveListener(listener);
            }

            pointerDownEvent = null;
            listener = null;
        }

        private static ReplicatorPointerButton MapButton(BaseEventData eventData)
        {
            var pointer = eventData as PointerEventData;
            if (pointer == null)
            {
                return ReplicatorPointerButton.Other;
            }

            switch (pointer.button)
            {
                case PointerEventData.InputButton.Left:
                    return ReplicatorPointerButton.Left;
                case PointerEventData.InputButton.Right:
                    return ReplicatorPointerButton.Right;
                case PointerEventData.InputButton.Middle:
                    return ReplicatorPointerButton.Middle;
                default:
                    return ReplicatorPointerButton.Other;
            }
        }
    }
}
