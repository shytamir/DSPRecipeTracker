using UnityEngine;
using UnityEngine.UI;

namespace DSPRecipeTracker
{
    internal sealed class UnityTrackerPanelAdapter : ITrackerPanelUiAdapter
    {
        private readonly RectTransform parent;
        private GameObject panelObject;
        private RectTransform panelTransform;
        private Image panelBackground;
        private readonly GameObject[] recipeIconObjects =
            new GameObject[PinnedRecipeState.Capacity];
        private readonly Image[] recipeIconImages =
            new Image[PinnedRecipeState.Capacity];
        private bool recipeIconsReleased;

        public UnityTrackerPanelAdapter(RectTransform parent)
        {
            this.parent = parent;
        }

        internal RectTransform PanelTransform => panelTransform;

        public bool TryCreate()
        {
            if (!ReferenceEquals(panelObject, null) ||
                ReferenceEquals(parent, null))
            {
                return false;
            }

            panelObject = new GameObject("DSP Recipe Tracker");
            panelTransform = (RectTransform)panelObject.AddComponent(typeof(RectTransform));
            panelBackground = (Image)panelObject.AddComponent(typeof(Image));
            if (ReferenceEquals(panelTransform, null) || ReferenceEquals(panelBackground, null))
            {
                return false;
            }

            panelTransform.SetParent(parent, false);
            panelBackground.color = new Color(0.015f, 0.035f, 0.09f, 0.78f);
            return true;
        }

        public bool TryApplyLayout(PanelRectangle rectangle)
        {
            if (ReferenceEquals(panelTransform, null))
            {
                return false;
            }

            var topLeft = new Vector2(0f, 1f);
            panelTransform.anchorMin = topLeft;
            panelTransform.anchorMax = topLeft;
            panelTransform.pivot = topLeft;
            panelTransform.anchoredPosition = new Vector2(rectangle.Left, -rectangle.Top);
            panelTransform.sizeDelta = new Vector2(rectangle.Width, rectangle.Height);
            return true;
        }

        public bool TryEnableRaycastContainment()
        {
            if (ReferenceEquals(panelBackground, null))
            {
                return false;
            }

            panelBackground.raycastTarget = true;
            return true;
        }

        public bool TryApplyVisibility(bool visible)
        {
            if (ReferenceEquals(panelObject, null))
            {
                return false;
            }

            panelObject.SetActive(visible);
            return true;
        }

        public bool TryApplyRecipeIcons(RecipeIconSlot[] slots, int count)
        {
            if (recipeIconsReleased ||
                ReferenceEquals(panelTransform, null) ||
                slots == null ||
                count < 0 ||
                count > PinnedRecipeState.Capacity)
            {
                return false;
            }

            if (ReferenceEquals(recipeIconObjects[0], null) && !TryCreateRecipeIconSlots())
            {
                DisableRecipeIcons();
                return false;
            }

            for (var index = 0; index < recipeIconObjects.Length; index++)
            {
                if (index < count)
                {
                    var sprite = slots[index].Icon.Value as Sprite;
                    if (ReferenceEquals(sprite, null))
                    {
                        DisableRecipeIcons();
                        return false;
                    }

                    recipeIconImages[index].sprite = sprite;
                    recipeIconObjects[index].SetActive(true);
                }
                else
                {
                    recipeIconObjects[index].SetActive(false);
                }
            }

            return true;
        }

        public void ReleaseRecipeIcons()
        {
            DisableRecipeIcons();
        }

        public void Release()
        {
            DisableRecipeIcons();
            var ownedPanel = panelObject;
            panelObject = null;
            panelTransform = null;
            panelBackground = null;

            if (!ReferenceEquals(ownedPanel, null))
            {
                Object.Destroy(ownedPanel);
            }
        }

        private bool TryCreateRecipeIconSlots()
        {
            const float slotSize = 52f;
            const float left = 16f;
            const float firstTop = 16f;
            const float rowSpacing = 76f;

            for (var index = 0; index < recipeIconObjects.Length; index++)
            {
                var slotObject = new GameObject("Recipe Icon Slot " + (index + 1));
                recipeIconObjects[index] = slotObject;
                var slotTransform = (RectTransform)slotObject.AddComponent(typeof(RectTransform));
                var slotImage = (Image)slotObject.AddComponent(typeof(Image));
                if (ReferenceEquals(slotTransform, null) || ReferenceEquals(slotImage, null))
                {
                    return false;
                }

                slotTransform.SetParent(panelTransform, false);
                var topLeft = new Vector2(0f, 1f);
                slotTransform.anchorMin = topLeft;
                slotTransform.anchorMax = topLeft;
                slotTransform.pivot = topLeft;
                slotTransform.anchoredPosition =
                    new Vector2(left, -(firstTop + (rowSpacing * index)));
                slotTransform.sizeDelta = new Vector2(slotSize, slotSize);
                slotImage.raycastTarget = false;
                slotObject.SetActive(false);
                recipeIconImages[index] = slotImage;
            }

            return true;
        }

        private void DisableRecipeIcons()
        {
            if (recipeIconsReleased)
            {
                return;
            }

            recipeIconsReleased = true;
            for (var index = 0; index < recipeIconObjects.Length; index++)
            {
                var ownedSlot = recipeIconObjects[index];
                recipeIconObjects[index] = null;
                recipeIconImages[index] = null;
                if (!ReferenceEquals(ownedSlot, null))
                {
                    Object.Destroy(ownedSlot);
                }
            }
        }
    }
}
