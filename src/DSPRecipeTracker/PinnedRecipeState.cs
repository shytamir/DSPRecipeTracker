using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DSPRecipeTracker
{
    internal enum TrackerDiagnosticLevel
    {
        Debug
    }

    internal interface ITrackerDiagnosticSink
    {
        void Write(TrackerDiagnosticLevel level, string message);
    }

    internal enum PinStateChangeKind
    {
        None,
        Pinned,
        Unpinned,
        UnavailableRemoved
    }

    internal readonly struct PinStateChange
    {
        public PinStateChange(
            PinStateChangeKind kind,
            int recipeId,
            int? evictedRecipeId = null)
        {
            Kind = kind;
            RecipeId = recipeId;
            EvictedRecipeId = evictedRecipeId;
        }

        public PinStateChangeKind Kind { get; }

        public int RecipeId { get; }

        public int? EvictedRecipeId { get; }

        public bool Changed => Kind != PinStateChangeKind.None;
    }

    internal sealed class PinnedRecipeState
    {
        public const int Capacity = 3;

        private readonly ITrackerDiagnosticSink diagnostics;
        private readonly List<int> recipeIds = new List<int>(Capacity);
        private readonly ReadOnlyCollection<int> readOnlyRecipeIds;

        public PinnedRecipeState(ITrackerDiagnosticSink diagnostics)
        {
            this.diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
            readOnlyRecipeIds = recipeIds.AsReadOnly();
        }

        public IReadOnlyList<int> RecipeIds => readOnlyRecipeIds;

        public PinStateChange Toggle(int recipeId)
        {
            var existingIndex = recipeIds.IndexOf(recipeId);
            if (existingIndex >= 0)
            {
                recipeIds.RemoveAt(existingIndex);
                ReportChange("unpin", recipeId, null);
                return new PinStateChange(PinStateChangeKind.Unpinned, recipeId);
            }

            int? evictedRecipeId = null;
            if (recipeIds.Count == Capacity)
            {
                evictedRecipeId = recipeIds[Capacity - 1];
                recipeIds.RemoveAt(Capacity - 1);
            }

            recipeIds.Insert(0, recipeId);
            ReportChange("pin", recipeId, evictedRecipeId);
            return new PinStateChange(PinStateChangeKind.Pinned, recipeId, evictedRecipeId);
        }

        public PinStateChange RemoveUnavailable(int recipeId)
        {
            var existingIndex = recipeIds.IndexOf(recipeId);
            if (existingIndex < 0)
            {
                return new PinStateChange(PinStateChangeKind.None, recipeId);
            }

            recipeIds.RemoveAt(existingIndex);
            ReportChange("remove-unavailable", recipeId, null);
            return new PinStateChange(PinStateChangeKind.UnavailableRemoved, recipeId);
        }

        private void ReportChange(string action, int recipeId, int? evictedRecipeId)
        {
            var message = "tracker-state action=" + action +
                " recipeId=" + recipeId +
                " order=[" + string.Join(",", recipeIds) + "]";
            if (evictedRecipeId.HasValue)
            {
                message += " evictedRecipeId=" + evictedRecipeId.Value;
            }

            diagnostics.Write(TrackerDiagnosticLevel.Debug, message);
        }
    }
}
