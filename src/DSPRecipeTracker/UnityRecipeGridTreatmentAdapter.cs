using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DSPRecipeTracker
{
    internal sealed class UnityRecipeGridTreatmentAdapter : IRecipeGridTreatmentAdapter
    {
        internal const string RecipeArrayFieldName = "recipeProtoArray";
        internal const int GridColumns = 14;
        internal const int GridRows = 8;
        internal const float CellSize = 46f;
        internal const int MarkerCapacity = PinnedRecipeState.Capacity;
        internal const float CornerInset = 3f;
        internal const float CornerLength = 10f;
        internal const float CornerThickness = 2f;

        private static readonly Color MarkerColor = new Color(0.2f, 0.75f, 0.25f, 0.95f);
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        private readonly UIReplicatorWindow window;
        private readonly FieldInfo recipeArrayField;
        private readonly GameObject[] markerObjects = new GameObject[MarkerCapacity];
        private readonly RectTransform[] markerTransforms = new RectTransform[MarkerCapacity];
        private bool released;

        public UnityRecipeGridTreatmentAdapter(UIReplicatorWindow window)
        {
            this.window = window;
            recipeArrayField = typeof(UIReplicatorWindow).GetField(RecipeArrayFieldName, PrivateInstance);
        }

        public bool TryInitialize()
        {
            if (released || ReferenceEquals(window, null) || recipeArrayField == null ||
                ReferenceEquals(window.recipeBg, null))
            {
                return false;
            }

            for (var markerIndex = 0; markerIndex < MarkerCapacity; markerIndex++)
            {
                if (!TryCreateMarker(markerIndex))
                {
                    ReleaseMarkers();
                    return false;
                }
            }

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
                ReferenceEquals(markerObjects[0], null))
            {
                return false;
            }

            var markerIndex = 0;
            for (var cellIndex = 0; cellIndex < states.Length; cellIndex++)
            {
                if (states[cellIndex] != RecipeGridTreatmentModel.PinnedMarkerState)
                {
                    continue;
                }

                if (cellIndex >= GridColumns * GridRows || markerIndex >= MarkerCapacity)
                {
                    return false;
                }

                var column = cellIndex % GridColumns;
                var row = cellIndex / GridColumns;
                markerTransforms[markerIndex].anchoredPosition =
                    new Vector2(column * CellSize, -row * CellSize);
                markerObjects[markerIndex].SetActive(true);
                markerIndex++;
            }

            for (; markerIndex < MarkerCapacity; markerIndex++)
            {
                markerObjects[markerIndex].SetActive(false);
            }

            return true;
        }

        public void Release()
        {
            if (released)
            {
                return;
            }

            released = true;
            ReleaseMarkers();
        }

        private bool TryCreateMarker(int markerIndex)
        {
            var markerObject = new GameObject("Pinned Recipe Corners " + (markerIndex + 1));
            var markerTransform = (RectTransform)markerObject.AddComponent(typeof(RectTransform));
            if (ReferenceEquals(markerTransform, null))
            {
                Object.Destroy(markerObject);
                return false;
            }

            markerTransform.SetParent(window.recipeBg.transform, false);
            var topLeft = new Vector2(0f, 1f);
            markerTransform.anchorMin = topLeft;
            markerTransform.anchorMax = topLeft;
            markerTransform.pivot = topLeft;
            markerTransform.sizeDelta = new Vector2(CellSize, CellSize);

            if (!TryCreateCorner(markerTransform, "Top Left", new Vector2(0f, 1f), new Vector2(1f, -1f)) ||
                !TryCreateCorner(markerTransform, "Top Right", new Vector2(1f, 1f), new Vector2(-1f, -1f)) ||
                !TryCreateCorner(markerTransform, "Bottom Left", new Vector2(0f, 0f), new Vector2(1f, 1f)) ||
                !TryCreateCorner(markerTransform, "Bottom Right", new Vector2(1f, 0f), new Vector2(-1f, 1f)))
            {
                Object.Destroy(markerObject);
                return false;
            }

            markerObject.SetActive(false);
            markerObjects[markerIndex] = markerObject;
            markerTransforms[markerIndex] = markerTransform;
            return true;
        }

        private static bool TryCreateCorner(
            RectTransform parent,
            string name,
            Vector2 anchor,
            Vector2 direction)
        {
            return TryCreateSegment(
                    parent,
                    name + " Horizontal",
                    anchor,
                    new Vector2(direction.x * CornerInset, direction.y * CornerInset),
                    new Vector2(CornerLength, CornerThickness)) &&
                TryCreateSegment(
                    parent,
                    name + " Vertical",
                    anchor,
                    new Vector2(direction.x * CornerInset, direction.y * CornerInset),
                    new Vector2(CornerThickness, CornerLength));
        }

        private static bool TryCreateSegment(
            RectTransform parent,
            string name,
            Vector2 anchor,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            var segmentObject = new GameObject(name);
            var segmentTransform = (RectTransform)segmentObject.AddComponent(typeof(RectTransform));
            var segmentImage = (Image)segmentObject.AddComponent(typeof(Image));
            if (ReferenceEquals(segmentTransform, null) || ReferenceEquals(segmentImage, null))
            {
                Object.Destroy(segmentObject);
                return false;
            }

            segmentTransform.SetParent(parent, false);
            segmentTransform.anchorMin = anchor;
            segmentTransform.anchorMax = anchor;
            segmentTransform.pivot = anchor;
            segmentTransform.anchoredPosition = anchoredPosition;
            segmentTransform.sizeDelta = sizeDelta;
            segmentImage.color = MarkerColor;
            segmentImage.raycastTarget = false;
            return true;
        }

        private void ReleaseMarkers()
        {
            for (var markerIndex = 0; markerIndex < MarkerCapacity; markerIndex++)
            {
                if (!ReferenceEquals(markerObjects[markerIndex], null))
                {
                    markerObjects[markerIndex].SetActive(false);
                    Object.Destroy(markerObjects[markerIndex]);
                    markerObjects[markerIndex] = null;
                }

                markerTransforms[markerIndex] = null;
            }
        }
    }
}
