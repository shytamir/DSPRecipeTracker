using System;

namespace DSPRecipeTracker
{
    internal sealed class TrackerOrchestrator : IDisposable
    {
        private readonly PinnedRecipeState state;
        private readonly ReplicatorPinInput pinInput;
        private readonly RecipeGridTreatment gridTreatment;
        private readonly ILiveRecipePresentation recipePresentation;
        private readonly MajorInterfaceVisibilityInput majorInterfaces;
        private readonly TrackerPanelUiBoundary panel;
        private readonly TrackerPanelDrag panelDrag;
        private readonly TrackerVisibilityControls controls;
        private readonly ITrackerDiagnosticSink diagnostics;
        private bool manualRequested = true;
        private bool initialized;
        private bool released;
        private bool hasVisibilityObservation;
        private bool lastVisibility;

        public TrackerOrchestrator(
            PinnedRecipeState state,
            ReplicatorPinInput pinInput,
            RecipeGridTreatment gridTreatment,
            ILiveRecipePresentation recipePresentation,
            MajorInterfaceVisibilityInput majorInterfaces,
            TrackerPanelUiBoundary panel,
            TrackerPanelDrag panelDrag,
            TrackerVisibilityControls controls,
            ITrackerDiagnosticSink diagnostics)
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
            this.pinInput = pinInput ?? throw new ArgumentNullException(nameof(pinInput));
            this.gridTreatment = gridTreatment ?? throw new ArgumentNullException(nameof(gridTreatment));
            this.recipePresentation = recipePresentation ?? throw new ArgumentNullException(nameof(recipePresentation));
            this.majorInterfaces = majorInterfaces ?? throw new ArgumentNullException(nameof(majorInterfaces));
            this.panel = panel ?? throw new ArgumentNullException(nameof(panel));
            this.panelDrag = panelDrag ?? throw new ArgumentNullException(nameof(panelDrag));
            this.controls = controls ?? throw new ArgumentNullException(nameof(controls));
            this.diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        }

        public bool ManualRequested => manualRequested;

        public bool TryInitialize(PanelRectangle initialRectangle)
        {
            if (released || initialized)
            {
                return false;
            }

            var panelAvailable = panel.TryInitialize(initialRectangle);
            var dragAvailable = panelDrag.TryInitialize();
            var inputAvailable = pinInput.TryInitialize();
            var treatmentAvailable = gridTreatment.TryInitialize();
            var presentationAvailable = recipePresentation.TryInitialize();
            var controlsAvailable = controls.TryInitialize(HidePanel, ToggleGlobal, manualRequested);
            initialized = true;

            diagnostics.Write(
                TrackerDiagnosticLevel.Debug,
                "tracker-orchestration action=initialize panel=" + Format(panelAvailable) +
                " drag=" + Format(dragAvailable) +
                " input=" + Format(inputAvailable) +
                " treatment=" + Format(treatmentAvailable) +
                " presentation=" + Format(presentationAvailable) +
                " controls=" + Format(controlsAvailable));
            Refresh();
            return true;
        }

        public void Refresh()
        {
            if (!initialized || released)
            {
                return;
            }

            recipePresentation.Refresh();
            gridTreatment.TryRefresh(state.RecipeIds);
            panelDrag.RefreshBounds();
            ApplyVisibility();
        }

        public void Dispose()
        {
            if (released)
            {
                return;
            }

            released = true;
            initialized = false;
            controls.Dispose();
            panelDrag.Dispose();
            recipePresentation.Dispose();
            gridTreatment.Dispose();
            pinInput.Dispose();
            panel.Dispose();
            diagnostics.Write(TrackerDiagnosticLevel.Debug, "tracker-orchestration action=release");
        }

        private void HidePanel()
        {
            SetManualRequested(false, "panel-hide");
        }

        private void ToggleGlobal()
        {
            SetManualRequested(!manualRequested, "global-toggle");
        }

        private void SetManualRequested(bool requested, string action)
        {
            if (!initialized || released || manualRequested == requested)
            {
                return;
            }

            manualRequested = requested;
            diagnostics.Write(
                TrackerDiagnosticLevel.Debug,
                "tracker-orchestration action=" + action +
                " manualRequested=" + Format(manualRequested));
            controls.TryApplyManualRequested(manualRequested);
            ApplyVisibility();
        }

        private void ApplyVisibility()
        {
            var hasRows = state.RecipeIds.Count != 0;
            var majorSnapshot = majorInterfaces.Read();
            var visible = MajorInterfaceVisibilityInput.ResolveTrackerVisibility(
                hasRows,
                manualRequested,
                majorSnapshot);

            if (!hasVisibilityObservation || lastVisibility != visible)
            {
                panel.TryApplyVisibility(visible);
                diagnostics.Write(
                    TrackerDiagnosticLevel.Debug,
                    "tracker-orchestration visibility=" + Format(visible) +
                    " hasRows=" + Format(hasRows) +
                    " manualRequested=" + Format(manualRequested) +
                    " majorAvailable=" + Format(majorSnapshot.IsAvailable) +
                    " majorActive=" + Format(majorSnapshot.IsActive));
                hasVisibilityObservation = true;
                lastVisibility = visible;
            }
        }

        private static string Format(bool value)
        {
            return value.ToString().ToLowerInvariant();
        }

        private static string Format(bool? value)
        {
            return value.HasValue ? Format(value.Value) : "unavailable";
        }
    }
}
