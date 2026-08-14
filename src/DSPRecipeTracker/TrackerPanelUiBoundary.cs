using System;

namespace DSPRecipeTracker
{
    internal interface ITrackerPanelUiAdapter
    {
        bool TryCreate();

        bool TryApplyLayout(PanelRectangle rectangle);

        bool TryEnableRaycastContainment();

        bool TryApplyVisibility(bool visible);

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
            if (!available)
            {
                return false;
            }

            var nextRectangle = PanelGeometry.MoveAndClamp(rectangle, delta, parent);
            try
            {
                if (!adapter.TryApplyLayout(nextRectangle))
                {
                    return FailSoftly();
                }

                rectangle = nextRectangle;
                return true;
            }
            catch (Exception)
            {
                return FailSoftly();
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
