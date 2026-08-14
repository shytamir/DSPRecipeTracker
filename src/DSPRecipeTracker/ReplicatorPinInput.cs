using System;

namespace DSPRecipeTracker
{
    internal enum ReplicatorPointerButton
    {
        Left,
        Right,
        Middle,
        Other
    }

    internal interface IReplicatorPinInputAdapter
    {
        bool TryAttach(Action<ReplicatorPointerButton> pointerDown);

        bool TryGetCurrentRecipe(out int gridIndex, out int recipeId);

        void Release();
    }

    internal sealed class ReplicatorPinInput : IDisposable
    {
        private readonly IReplicatorPinInputAdapter adapter;
        private readonly PinnedRecipeState state;
        private readonly ITrackerDiagnosticSink diagnostics;
        private bool available;
        private bool released;
        private bool failureReported;
        private bool attached;

        public ReplicatorPinInput(
            IReplicatorPinInputAdapter adapter,
            PinnedRecipeState state,
            ITrackerDiagnosticSink diagnostics)
        {
            this.adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            this.state = state ?? throw new ArgumentNullException(nameof(state));
            this.diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        }

        public bool IsAvailable => available;

        public bool TryInitialize()
        {
            if (released || available)
            {
                return false;
            }

            try
            {
                if (!adapter.TryAttach(OnPointerDown))
                {
                    return FailSoftly("attach");
                }

                available = true;
                attached = true;
                diagnostics.Write(TrackerDiagnosticLevel.Debug, "replicator-pin-input action=attach");
                return true;
            }
            catch (Exception)
            {
                return FailSoftly("attach");
            }
        }

        public void Dispose()
        {
            ReleaseAdapter();
        }

        private void OnPointerDown(ReplicatorPointerButton button)
        {
            if (!available || button != ReplicatorPointerButton.Right)
            {
                return;
            }

            try
            {
                if (!adapter.TryGetCurrentRecipe(out var gridIndex, out var recipeId))
                {
                    FailSoftly("recipe");
                    return;
                }

                var change = state.Toggle(recipeId);
                diagnostics.Write(
                    TrackerDiagnosticLevel.Debug,
                    "replicator-pin-input action=" +
                    (change.Kind == PinStateChangeKind.Pinned ? "pin" : "unpin") +
                    " gridIndex=" + gridIndex +
                    " recipeId=" + recipeId);
            }
            catch (Exception)
            {
                FailSoftly("recipe");
            }
        }

        private bool FailSoftly(string stage)
        {
            if (!failureReported)
            {
                diagnostics.Write(
                    TrackerDiagnosticLevel.Debug,
                    "replicator-pin-input action=disable stage=" + stage);
                failureReported = true;
            }

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
                // Cleanup remains best-effort after a missing or changed game member.
            }

            if (attached)
            {
                diagnostics.Write(TrackerDiagnosticLevel.Debug, "replicator-pin-input action=detach");
                attached = false;
            }
        }
    }
}
