using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DSPRecipeTracker
{
    internal sealed class UnityTrackerVisibilityControlAdapter : ITrackerVisibilityControlAdapter
    {
        internal const string ControlTitle = "Recipe Tracker";
        internal const string HideCopy = "Hide Recipe Tracker";
        internal const string ShowCopy = "Show Recipe Tracker";

        private readonly RectTransform globalParent;
        private readonly Button nativeTemplate;
        private readonly Sprite nativeReplicatorIcon;
        private readonly UnityTrackerPanelAdapter panel;
        private Button panelHideButton;
        private Button globalToggleButton;
        private UIButton globalToggleUiButton;
        private UnityAction panelHideListener;
        private UnityAction globalToggleListener;
        private bool released;

        public UnityTrackerVisibilityControlAdapter(
            RectTransform globalParent,
            Button nativeTemplate,
            Sprite nativeReplicatorIcon,
            UnityTrackerPanelAdapter panel)
        {
            this.globalParent = globalParent;
            this.nativeTemplate = nativeTemplate;
            this.nativeReplicatorIcon = nativeReplicatorIcon;
            this.panel = panel;
        }

        public bool TryCreate(Action hidePanel, Action toggleGlobal, bool manualRequested)
        {
            if (released || hidePanel == null || toggleGlobal == null ||
                ReferenceEquals(globalParent, null) || ReferenceEquals(nativeTemplate, null) ||
                ReferenceEquals(nativeReplicatorIcon, null) || ReferenceEquals(panel, null) ||
                ReferenceEquals(panel.PanelTransform, null))
            {
                return false;
            }

            globalToggleButton = Object.Instantiate(nativeTemplate, globalParent, false);
            panelHideButton = Object.Instantiate(nativeTemplate, panel.PanelTransform, false);
            if (ReferenceEquals(globalToggleButton, null) || ReferenceEquals(panelHideButton, null))
            {
                return false;
            }

            panelHideListener = () =>
            {
                if (!released)
                {
                    hidePanel();
                }
            };
            globalToggleListener = () =>
            {
                if (!released)
                {
                    toggleGlobal();
                }
            };

            if (!TryConfigureControl(panelHideButton, panelHideListener, HideCopy, out var panelUiButton) ||
                !TryConfigureControl(globalToggleButton, globalToggleListener, HideCopy, out globalToggleUiButton))
            {
                return false;
            }

            PositionPanelControl(panelHideButton);
            PositionGlobalControl(globalToggleButton);
            panelUiButton.tips.tipText = HideCopy;
            return TryApplyManualRequested(manualRequested);
        }

        public bool TryApplyManualRequested(bool manualRequested)
        {
            if (released || ReferenceEquals(globalToggleButton, null) ||
                ReferenceEquals(globalToggleUiButton, null))
            {
                return false;
            }

            var actionCopy = manualRequested ? HideCopy : ShowCopy;
            globalToggleButton.gameObject.name = ControlTitle + " - " + actionCopy;
            globalToggleUiButton.tips.tipTitle = ControlTitle;
            globalToggleUiButton.tips.tipText = actionCopy;
            return true;
        }

        public void Release()
        {
            if (released)
            {
                return;
            }

            released = true;
            ReleaseButton(ref panelHideButton, panelHideListener);
            ReleaseButton(ref globalToggleButton, globalToggleListener);
            panelHideListener = null;
            globalToggleListener = null;
            globalToggleUiButton = null;
        }

        private bool TryConfigureControl(
            Button button,
            UnityAction listener,
            string actionCopy,
            out UIButton uiButton)
        {
            uiButton = button.GetComponent<UIButton>();
            var iconImage = FindNonRaycastingIcon(button);
            if (ReferenceEquals(iconImage, null) || ReferenceEquals(uiButton, null))
            {
                return false;
            }

            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(listener);
            iconImage.sprite = nativeReplicatorIcon;
            button.gameObject.name = ControlTitle + " - " + actionCopy;

            uiButton.tipTitleFormatString = string.Empty;
            uiButton.tipTextFormatString = string.Empty;
            uiButton.tips.tipSprite = null;
            uiButton.tips.itemId = 0;
            uiButton.tips.itemCount = 0;
            uiButton.tips.itemInc = 0;
            uiButton.tips.type = UIButton.ItemTipType.None;
            uiButton.tips.tipTitle = ControlTitle;
            uiButton.tips.tipText = actionCopy;

            var localizers = button.GetComponentsInChildren<Localizer>(true);
            for (var index = 0; index < localizers.Length; index++)
            {
                Object.Destroy(localizers[index]);
            }

            return true;
        }

        internal static Sprite TryResolveNativeIcon(Button button)
        {
            var icon = FindNonRaycastingIcon(button);
            return ReferenceEquals(icon, null) ? null : icon.sprite;
        }

        private static Image FindNonRaycastingIcon(Button button)
        {
            if (ReferenceEquals(button, null))
            {
                return null;
            }

            var images = button.GetComponentsInChildren<Image>(true);
            for (var index = 0; index < images.Length; index++)
            {
                if (!images[index].raycastTarget && !ReferenceEquals(images[index].sprite, null))
                {
                    return images[index];
                }
            }

            return null;
        }

        private static void PositionPanelControl(Button button)
        {
            var transform = (RectTransform)button.transform;
            var topRight = new Vector2(1f, 1f);
            transform.anchorMin = topRight;
            transform.anchorMax = topRight;
            transform.pivot = topRight;
            transform.anchoredPosition = new Vector2(-8f, -8f);
            transform.sizeDelta = new Vector2(36f, 36f);
        }

        private void PositionGlobalControl(Button button)
        {
            var source = (RectTransform)nativeTemplate.transform;
            var transform = (RectTransform)button.transform;
            transform.anchoredPosition = source.anchoredPosition + new Vector2(0f, 38f);
        }

        private static void ReleaseButton(ref Button button, UnityAction listener)
        {
            var ownedButton = button;
            button = null;
            if (ReferenceEquals(ownedButton, null))
            {
                return;
            }

            if (listener != null)
            {
                ownedButton.onClick.RemoveListener(listener);
            }

            Object.Destroy(ownedButton.gameObject);
        }
    }
}
