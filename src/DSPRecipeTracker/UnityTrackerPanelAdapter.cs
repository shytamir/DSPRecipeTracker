using UnityEngine;
using UnityEngine.UI;

namespace DSPRecipeTracker
{
    internal sealed class UnityTrackerPanelAdapter : ITrackerPanelUiAdapter
    {
        private readonly RectTransform parent;
        private readonly Sprite backgroundSprite;
        private GameObject panelObject;
        private RectTransform panelTransform;
        private Image panelBackground;

        public UnityTrackerPanelAdapter(RectTransform parent, Sprite backgroundSprite)
        {
            this.parent = parent;
            this.backgroundSprite = backgroundSprite;
        }

        public bool TryCreate()
        {
            if (!ReferenceEquals(panelObject, null) ||
                ReferenceEquals(parent, null) ||
                ReferenceEquals(backgroundSprite, null))
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
            panelBackground.sprite = backgroundSprite;
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

        public void Release()
        {
            var ownedPanel = panelObject;
            panelObject = null;
            panelTransform = null;
            panelBackground = null;

            if (!ReferenceEquals(ownedPanel, null))
            {
                Object.Destroy(ownedPanel);
            }
        }
    }
}
