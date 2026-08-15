using System;

namespace DSPRecipeTracker
{
    internal interface ITrackerPanelUiAdapter
    {
        bool TryCreate();

        bool TryApplyLayout(PanelRectangle rectangle);

        bool TryEnableRaycastContainment();

        bool TryApplyVisibility(bool visible);

        bool TryApplyRecipeIcons(RecipeIconSlot[] slots, int count);

        void ReleaseRecipeIcons();

        void Release();
    }

    internal sealed class TrackerPanelUiBoundary : IDisposable
    {
        private readonly ITrackerPanelUiAdapter adapter;
        private PanelRectangle rectangle;
        private bool available;
        private bool released;

        public TrackerPanelUiBoundary(ITrackerPanelUiAdapter adapter)
        {
            this.adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }

        public bool IsAvailable => available;

        public bool TryInitialize(PanelRectangle initialRectangle)
        {
            if (released || available)
            {
                return false;
            }

            try
            {
                if (!adapter.TryCreate() ||
                    !adapter.TryApplyLayout(initialRectangle) ||
                    !adapter.TryEnableRaycastContainment())
                {
                    return FailSoftly();
                }

                rectangle = initialRectangle;
                available = true;
                return true;
            }
            catch (Exception)
            {
                return FailSoftly();
            }
        }

        public bool TryApplyDrag(DragDelta delta, ParentBounds parent)
        {
            return TryApplyDrag(delta, parent, out _);
        }

        public bool TryApplyDrag(DragDelta delta, ParentBounds parent, out bool clamped)
        {
            clamped = false;
            if (!available)
            {
                return false;
            }

            var nextRectangle = PanelGeometry.MoveAndClamp(rectangle, delta, parent);
            clamped = nextRectangle.Left != rectangle.Left + delta.Horizontal ||
                nextRectangle.Top != rectangle.Top + delta.Vertical;
            try
            {
                if (!adapter.TryApplyLayout(nextRectangle))
                {
                    return false;
                }

                rectangle = nextRectangle;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryReclamp(ParentBounds parent, out bool corrected)
        {
            corrected = false;
            if (!available)
            {
                return false;
            }

            var nextRectangle = PanelGeometry.Clamp(rectangle, parent);
            corrected = nextRectangle.Left != rectangle.Left || nextRectangle.Top != rectangle.Top;
            if (!corrected)
            {
                return true;
            }

            try
            {
                if (!adapter.TryApplyLayout(nextRectangle))
                {
                    return false;
                }

                rectangle = nextRectangle;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryApplyVisibility(bool visible)
        {
            if (!available)
            {
                return false;
            }

            try
            {
                return adapter.TryApplyVisibility(visible) || FailSoftly();
            }
            catch (Exception)
            {
                return FailSoftly();
            }
        }

        public bool TryApplyRecipeIcons(RecipeIconSlot[] slots, int count)
        {
            if (!available || slots == null || count < 0 || count > PinnedRecipeState.Capacity)
            {
                return false;
            }

            try
            {
                return adapter.TryApplyRecipeIcons(slots, count);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ReleaseRecipeIcons()
        {
            if (released)
            {
                return;
            }

            try
            {
                adapter.ReleaseRecipeIcons();
            }
            catch (Exception)
            {
                // Slot cleanup remains isolated from the panel shell.
            }
        }

        public void Dispose()
        {
            ReleaseAdapter();
        }

        private bool FailSoftly()
        {
            ReleaseAdapter();
            return false;
        }

        private void ReleaseAdapter()
        {
            if (released)
            {
                return;
            }

            available = false;
            released = true;
            try
            {
                adapter.Release();
            }
            catch (Exception)
            {
                // Runtime cleanup remains best-effort when the adapter is unavailable.
            }
        }
    }
}
