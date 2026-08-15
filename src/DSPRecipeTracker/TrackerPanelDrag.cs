using System;
using System.Globalization;

namespace DSPRecipeTracker
{
    internal interface ITrackerPanelDragAdapter
    {
        bool TryAttach(Action<DragDelta> drag, Action dragCompleted);

        bool TryReadScaleFactor(out float scaleFactor);

        bool TryReadParentSize(out float width, out float height);

        void Release();
    }

    internal static class TrackerPanelDragGeometry
    {
        public static bool TryConvertScreenDelta(
            DragDelta screenDelta,
            float scaleFactor,
            out DragDelta layoutDelta)
        {
            layoutDelta = default(DragDelta);
            if (!IsFinite(screenDelta.Horizontal) ||
                !IsFinite(screenDelta.Vertical) ||
                !IsFinite(scaleFactor) ||
                scaleFactor <= 0f)
            {
                return false;
            }

            layoutDelta = new DragDelta(
                screenDelta.Horizontal / scaleFactor,
                screenDelta.Vertical / scaleFactor);
            return true;
        }

        public static bool TryCreateParentBounds(
            float width,
            float height,
            out ParentBounds bounds)
        {
            bounds = default(ParentBounds);
            if (!IsFinite(width) || !IsFinite(height) || width <= 0f || height <= 0f)
            {
                return false;
            }

            bounds = new ParentBounds(0f, 0f, width, height);
            return true;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }

    internal sealed class TrackerPanelDrag : IDisposable
    {
        private readonly ITrackerPanelDragAdapter adapter;
        private readonly TrackerPanelUiBoundary panel;
        private readonly ITrackerDiagnosticSink diagnostics;
        private ParentBounds bounds;
        private bool available;
        private bool released;
        private bool hasBounds;
        private bool dragMoved;
        private bool dragClamped;

        public TrackerPanelDrag(
            ITrackerPanelDragAdapter adapter,
            TrackerPanelUiBoundary panel,
            ITrackerDiagnosticSink diagnostics)
        {
            this.adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            this.panel = panel ?? throw new ArgumentNullException(nameof(panel));
            this.diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        }

        public bool IsAvailable => available;

        public bool TryInitialize()
        {
            if (released || available || !panel.IsAvailable)
            {
                return false;
            }

            try
            {
                if (!TryRefreshBounds(false) || !adapter.TryAttach(ApplyDrag, CompleteDrag))
                {
                    return Disable("initialize");
                }

                available = true;
                diagnostics.Write(
                    TrackerDiagnosticLevel.Debug,
                    "tracker-drag action=initialize bounds=" + Format(bounds.Width) + "x" + Format(bounds.Height));
                return true;
            }
            catch (Exception)
            {
                return Disable("initialize");
            }
        }

        public void RefreshBounds()
        {
            if (!available || released)
            {
                return;
            }

            try
            {
                if (!TryRefreshBounds(true))
                {
                    Disable("bounds");
                }
            }
            catch (Exception)
            {
                Disable("bounds");
            }
        }

        public void Dispose()
        {
            if (released)
            {
                return;
            }

            released = true;
            available = false;
            TryReleaseAdapter();
            diagnostics.Write(TrackerDiagnosticLevel.Debug, "tracker-drag action=release");
        }

        private void ApplyDrag(DragDelta screenDelta)
        {
            if (!available || released)
            {
                return;
            }

            try
            {
                if (!TryRefreshBounds(true))
                {
                    Disable("bounds");
                    return;
                }

                if (!adapter.TryReadScaleFactor(out var scaleFactor) ||
                    !TrackerPanelDragGeometry.TryConvertScreenDelta(
                        screenDelta,
                        scaleFactor,
                        out var layoutDelta))
                {
                    Disable("scale");
                    return;
                }

                if (!panel.TryApplyDrag(layoutDelta, bounds, out var clamped))
                {
                    Disable("layout");
                    return;
                }

                dragMoved = true;
                dragClamped |= clamped;
            }
            catch (Exception)
            {
                Disable("drag");
            }
        }

        private void CompleteDrag()
        {
            if (!available || released || !dragMoved)
            {
                return;
            }

            diagnostics.Write(
                TrackerDiagnosticLevel.Debug,
                "tracker-drag action=complete clamped=" + dragClamped.ToString().ToLowerInvariant());
            dragMoved = false;
            dragClamped = false;
        }

        private bool TryRefreshBounds(bool reportChange)
        {
            if (!adapter.TryReadParentSize(out var width, out var height) ||
                !TrackerPanelDragGeometry.TryCreateParentBounds(width, height, out var nextBounds))
            {
                return false;
            }

            if (hasBounds && bounds.Width == nextBounds.Width && bounds.Height == nextBounds.Height)
            {
                return true;
            }

            if (!panel.TryReclamp(nextBounds, out var corrected))
            {
                return false;
            }

            bounds = nextBounds;
            hasBounds = true;
            if (reportChange)
            {
                diagnostics.Write(
                    TrackerDiagnosticLevel.Debug,
                    "tracker-drag action=bounds width=" + Format(width) + " height=" + Format(height));
            }

            if (corrected)
            {
                diagnostics.Write(
                    TrackerDiagnosticLevel.Debug,
                    "tracker-drag action=clamp-correction source=bounds");
            }

            return true;
        }

        private bool Disable(string reason)
        {
            if (released)
            {
                return false;
            }

            released = true;
            available = false;
            TryReleaseAdapter();
            diagnostics.Write(
                TrackerDiagnosticLevel.Debug,
                "tracker-drag action=disable reason=" + reason);
            return false;
        }

        private void TryReleaseAdapter()
        {
            try
            {
                adapter.Release();
            }
            catch (Exception)
            {
                // Tracker presentation remains usable when drag cleanup is unavailable.
            }
        }

        private static string Format(float value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
