using System;

namespace DSPRecipeTracker
{
    internal interface ITrackerVisibilityControlAdapter
    {
        bool TryCreate(Action hidePanel, Action toggleGlobal, bool manualRequested);

        bool TryApplyManualRequested(bool manualRequested);

        void Release();
    }

    internal sealed class TrackerVisibilityControls : IDisposable
    {
        private readonly ITrackerVisibilityControlAdapter adapter;
        private bool available;
        private bool released;

        public TrackerVisibilityControls(ITrackerVisibilityControlAdapter adapter)
        {
            this.adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }

        public bool IsAvailable => available;

        public bool TryInitialize(Action hidePanel, Action toggleGlobal, bool manualRequested)
        {
            if (released || available || hidePanel == null || toggleGlobal == null)
            {
                return false;
            }

            try
            {
                if (!adapter.TryCreate(hidePanel, toggleGlobal, manualRequested))
                {
                    return FailSoftly();
                }

                available = true;
                return true;
            }
            catch (Exception)
            {
                return FailSoftly();
            }
        }

        public bool TryApplyManualRequested(bool manualRequested)
        {
            if (!available)
            {
                return false;
            }

            try
            {
                return adapter.TryApplyManualRequested(manualRequested) || FailSoftly();
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
                // Runtime cleanup remains best-effort when a native control is unavailable.
            }
        }
    }
}
