#if NET8_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Generic;
using System.Text;

namespace DSPRecipeTracker
{
    internal interface ILiveRecipePresentation : IDisposable
    {
        bool TryInitialize();

        void Refresh();
    }

    internal sealed class LiveRecipePresentation : ILiveRecipePresentation
    {
        internal const int SteadyRefreshCallInterval = 12;

        private static readonly IReadOnlyList<RecipePresentationInput> EmptyInputs =
            new RecipePresentationInput[0];
        private static readonly int[] EmptyIds = new int[0];

        private readonly PinnedRecipeState state;
        private readonly RecipePresentationInputSource inputSource;
        private readonly RecipePresentationModel model;
        private readonly RecipeRowPresentation rows;
        private readonly ITrackerDiagnosticSink diagnostics;
        private readonly int[] observedRecipeIds = new int[PinnedRecipeState.Capacity];
        private readonly int[] observedSuppressedIds = new int[PinnedRecipeState.Capacity];
        private int observedRecipeCount = -1;
        private int observedSuppressedCount = -1;
        private int refreshCallsRemaining;
        private bool? rowPresentationAvailable;
        private bool initialized;
        private bool available;
        private bool requiresRowRetry;
        private bool released;

        public LiveRecipePresentation(
            PinnedRecipeState state,
            RecipePresentationInputSource inputSource,
            RecipePresentationModel model,
            RecipeRowPresentation rows,
            ITrackerDiagnosticSink diagnostics)
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
            this.inputSource = inputSource ?? throw new ArgumentNullException(nameof(inputSource));
            this.model = model ?? throw new ArgumentNullException(nameof(model));
            this.rows = rows ?? throw new ArgumentNullException(nameof(rows));
            this.diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        }

        public bool TryInitialize()
        {
            if (released || initialized)
            {
                return false;
            }

            initialized = true;
            available = rows.TryInitialize();
            if (!available)
            {
                diagnostics.Write(
                    TrackerDiagnosticLevel.Debug,
                    "live-recipe-refresh action=disable stage=rows");
                return false;
            }

            diagnostics.Write(
                TrackerDiagnosticLevel.Debug,
                "live-recipe-refresh action=initialize interval=" +
                SteadyRefreshCallInterval);
            return true;
        }

        public void Refresh()
        {
            if (!initialized || !available || released)
            {
                return;
            }

            var pinsChanged = HavePinsChanged();
            if (!pinsChanged && observedRecipeCount == 0 && !requiresRowRetry)
            {
                return;
            }

            if (!pinsChanged && refreshCallsRemaining > 0)
            {
                refreshCallsRemaining--;
                return;
            }

            CachePins();
            refreshCallsRemaining = SteadyRefreshCallInterval - 1;

            if (observedRecipeCount == 0)
            {
                Apply(
                    model.Build(EmptyInputs),
                    EmptyIds,
                    EmptyIds);
                return;
            }

            var collection = inputSource.Collect();
            if (collection.IsReleased)
            {
                Disable("data-source");
                return;
            }

            Apply(
                model.Build(collection.Inputs),
                collection.SuppressedRecipeIds,
                collection.RemovedRecipeIds);
            CachePins();
        }

        public void Dispose()
        {
            if (released)
            {
                return;
            }

            released = true;
            initialized = false;
            available = false;
            try
            {
                rows.Dispose();
            }
            catch
            {
            }

            try
            {
                inputSource.Dispose();
            }
            catch
            {
            }

            diagnostics.Write(
                TrackerDiagnosticLevel.Debug,
                "live-recipe-refresh action=release");
        }

        private void Apply(
            RecipePresentationBuildResult result,
            IReadOnlyList<int> suppressedRecipeIds,
            IReadOnlyList<int> removedRecipeIds)
        {
            var suppressionChanged = HaveSuppressedRowsChanged(suppressedRecipeIds);
            CacheSuppressedRows(suppressedRecipeIds);

            var attempted = result.Changed || requiresRowRetry;
            var applied = true;
            if (attempted)
            {
                applied = rows.TryApplyFrame(result.Frame);
                requiresRowRetry = !applied;
            }

            var rowAvailabilityChanged = !rowPresentationAvailable.HasValue ||
                rowPresentationAvailable.Value != applied;
            rowPresentationAvailable = applied;

            if (result.Changed || suppressionChanged ||
                removedRecipeIds.Count != 0 || rowAvailabilityChanged)
            {
                ReportRefresh(
                    result,
                    suppressedRecipeIds,
                    removedRecipeIds,
                    attempted,
                    applied);
            }
        }

        private bool HavePinsChanged()
        {
            var recipeIds = state.RecipeIds;
            if (recipeIds.Count != observedRecipeCount)
            {
                return true;
            }

            for (var index = 0; index < recipeIds.Count; index++)
            {
                if (recipeIds[index] != observedRecipeIds[index])
                {
                    return true;
                }
            }

            return false;
        }

        private void CachePins()
        {
            var recipeIds = state.RecipeIds;
            observedRecipeCount = recipeIds.Count;
            for (var index = 0; index < observedRecipeIds.Length; index++)
            {
                observedRecipeIds[index] = index < recipeIds.Count
                    ? recipeIds[index]
                    : 0;
            }
        }

        private bool HaveSuppressedRowsChanged(IReadOnlyList<int> recipeIds)
        {
            if (recipeIds.Count != observedSuppressedCount)
            {
                return true;
            }

            for (var index = 0; index < recipeIds.Count; index++)
            {
                if (recipeIds[index] != observedSuppressedIds[index])
                {
                    return true;
                }
            }

            return false;
        }

        private void CacheSuppressedRows(IReadOnlyList<int> recipeIds)
        {
            observedSuppressedCount = recipeIds.Count;
            for (var index = 0; index < observedSuppressedIds.Length; index++)
            {
                observedSuppressedIds[index] = index < recipeIds.Count
                    ? recipeIds[index]
                    : 0;
            }
        }

        private void Disable(string stage)
        {
            available = false;
            diagnostics.Write(
                TrackerDiagnosticLevel.Debug,
                "live-recipe-refresh action=disable stage=" + stage);
        }

        private void ReportRefresh(
            RecipePresentationBuildResult result,
            IReadOnlyList<int> suppressedRecipeIds,
            IReadOnlyList<int> removedRecipeIds,
            bool attempted,
            bool applied)
        {
            var message = new StringBuilder("live-recipe-refresh action=refresh pins=");
            message.Append(state.RecipeIds.Count);
            message.Append(" rows=");
            message.Append(result.Frame.Rows.Count);
            message.Append(" suppressed=");
            AppendIds(message, suppressedRecipeIds);
            message.Append(" removed=");
            AppendIds(message, removedRecipeIds);
            message.Append(" changed=");
            message.Append(Format(result.Changed));
            message.Append(" uiAttempted=");
            message.Append(Format(attempted));
            message.Append(" uiAvailable=");
            message.Append(Format(applied));
            diagnostics.Write(TrackerDiagnosticLevel.Debug, message.ToString());
        }

        private static void AppendIds(StringBuilder message, IReadOnlyList<int> recipeIds)
        {
            message.Append('[');
            var count = Math.Min(recipeIds.Count, PinnedRecipeState.Capacity);
            for (var index = 0; index < count; index++)
            {
                if (index != 0)
                {
                    message.Append(',');
                }

                message.Append(recipeIds[index]);
            }

            message.Append(']');
        }

        private static string Format(bool value)
        {
            return value.ToString().ToLowerInvariant();
        }
    }
}
