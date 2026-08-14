using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DSPRecipeTracker
{
    internal sealed class UnityRecipeGridTreatmentAdapter : IRecipeGridTreatmentAdapter
    {
        internal const string RecipeArrayFieldName = "recipeProtoArray";
        internal const float OverlayOpacity = 0.08f;

        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private const string StateBufferProperty = "_StateBuffer";
        private const string FilterColorProperty = "_FilterColor";
        private const string BansColorProperty = "_BansColor";

        private readonly UIReplicatorWindow window;
        private readonly FieldInfo recipeArrayField;
        private Image overlay;
        private Material material;
        private ComputeBuffer stateBuffer;
        private bool released;

        public UnityRecipeGridTreatmentAdapter(UIReplicatorWindow window)
        {
            this.window = window;
            recipeArrayField = typeof(UIReplicatorWindow).GetField(RecipeArrayFieldName, PrivateInstance);
        }

        public bool TryInitialize()
        {
            if (released || ReferenceEquals(window, null) || recipeArrayField == null ||
                ReferenceEquals(window.recipeBg, null) || ReferenceEquals(window.recipeIcons, null))
            {
                return false;
            }

            var nativeMaterial = window.recipeBg.material;
            if (ReferenceEquals(nativeMaterial, null))
            {
                return false;
            }

            overlay = Object.Instantiate(window.recipeBg, window.recipeBg.transform.parent, false);
            if (ReferenceEquals(overlay, null))
            {
                return false;
            }

            overlay.raycastTarget = false;
            overlay.color = new Color(1f, 1f, 1f, OverlayOpacity);
            overlay.transform.SetSiblingIndex(window.recipeIcons.transform.GetSiblingIndex());

            var copiedTrigger = overlay.GetComponent<EventTrigger>();
            if (!ReferenceEquals(copiedTrigger, null))
            {
                Object.Destroy(copiedTrigger);
            }

            material = new Material(nativeMaterial);
            stateBuffer = new ComputeBuffer(RecipeGridTreatmentModel.CellCount, sizeof(uint));
            material.SetBuffer(StateBufferProperty, stateBuffer);
            material.SetColor(FilterColorProperty, new Color(0.2f, 0.75f, 0.25f, 1f));
            material.SetColor(BansColorProperty, new Color(0.78f, 0.22f, 0.22f, 0.45f));
            overlay.material = material;
            return true;
        }

        public bool TryReadPopulation(int[] recipeIds)
        {
            if (released || recipeIds == null || recipeIds.Length != RecipeGridTreatmentModel.CellCount ||
                ReferenceEquals(window, null) || recipeArrayField == null)
            {
                return false;
            }

            var recipes = recipeArrayField.GetValue(window) as RecipeProto[];
            if (recipes == null || recipes.Length != recipeIds.Length)
            {
                return false;
            }

            for (var index = 0; index < recipeIds.Length; index++)
            {
                var recipe = recipes[index];
                recipeIds[index] = recipe == null ? 0 : recipe.ID;
            }

            return true;
        }

        public bool TryApplyState(uint[] states)
        {
            if (released || states == null || states.Length != RecipeGridTreatmentModel.CellCount ||
                stateBuffer == null)
            {
                return false;
            }

            stateBuffer.SetData(states);
            return true;
        }

        public void Release()
        {
            if (released)
            {
                return;
            }

            released = true;
            if (!ReferenceEquals(overlay, null))
            {
                overlay.gameObject.SetActive(false);
            }

            if (stateBuffer != null)
            {
                stateBuffer.Release();
                stateBuffer = null;
            }

            if (!ReferenceEquals(material, null))
            {
                Object.Destroy(material);
                material = null;
            }

            if (!ReferenceEquals(overlay, null))
            {
                Object.Destroy(overlay.gameObject);
                overlay = null;
            }
        }
    }
}
