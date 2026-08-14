using System;
using System.Collections.Generic;

namespace DSPRecipeTracker
{
    internal readonly struct RecipeIconHandle
    {
        public RecipeIconHandle(object value)
        {
            Value = value;
        }

        public object Value { get; }

        public bool IsAvailable => !ReferenceEquals(Value, null);
    }

    internal readonly struct RecipeIconSlot
    {
        public RecipeIconSlot(int recipeId, RecipeIconHandle icon)
        {
            RecipeId = recipeId;
            Icon = icon;
        }

        public int RecipeId { get; }

        public RecipeIconHandle Icon { get; }
    }

    internal interface IRecipeIconResolver
    {
        bool TryResolve(int recipeId, out RecipeIconHandle icon);
    }

    internal sealed class RecipeIconSlotPresentation : IDisposable
    {
        private readonly PinnedRecipeState pinnedRecipes;
        private readonly IRecipeIconResolver resolver;
        private readonly TrackerPanelUiBoundary panel;
        private readonly ITrackerDiagnosticSink diagnostics;
        private readonly RecipeIconSlot[] resolvedSlots =
            new RecipeIconSlot[PinnedRecipeState.Capacity];
        private readonly int[] appliedRecipeIds = new int[PinnedRecipeState.Capacity];
        private readonly HashSet<int> reportedInvalidRecipeIds = new HashSet<int>();
        private int appliedCount = -1;
        private bool available;
        private bool released;
        private bool slotsReleased;

        public RecipeIconSlotPresentation(
            PinnedRecipeState pinnedRecipes,
            IRecipeIconResolver resolver,
            TrackerPanelUiBoundary panel,
            ITrackerDiagnosticSink diagnostics)
        {
            this.pinnedRecipes = pinnedRecipes ?? throw new ArgumentNullException(nameof(pinnedRecipes));
            this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            this.panel = panel ?? throw new ArgumentNullException(nameof(panel));
            this.diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        }

        public bool IsAvailable => available;

        public bool TryInitialize()
        {
            if (released || available)
            {
                return false;
            }

            if (!panel.IsAvailable)
            {
                released = true;
                diagnostics.Write(
                    TrackerDiagnosticLevel.Debug,
                    "recipe-icon-slots action=disable reason=panel-unavailable");
                return false;
            }

            available = true;
            diagnostics.Write(
                TrackerDiagnosticLevel.Debug,
                "recipe-icon-slots action=initialize");
            return true;
        }

        public bool TryRefresh()
        {
            if (!available)
            {
                return false;
            }

            var resolvedCount = 0;
            var pinIndex = 0;
            while (pinIndex < pinnedRecipes.RecipeIds.Count &&
                resolvedCount < PinnedRecipeState.Capacity)
            {
                var recipeId = pinnedRecipes.RecipeIds[pinIndex];
                RecipeIconHandle icon;
                var resolved = false;
                try
                {
                    resolved = resolver.TryResolve(recipeId, out icon);
                }
                catch
                {
                    icon = default(RecipeIconHandle);
                }

                if (!resolved || !icon.IsAvailable)
                {
                    pinnedRecipes.RemoveUnavailable(recipeId);
                    ReportInvalidOnce(recipeId);
                    continue;
                }

                resolvedSlots[resolvedCount] = new RecipeIconSlot(recipeId, icon);
                resolvedCount++;
                pinIndex++;
            }

            for (var index = resolvedCount; index < resolvedSlots.Length; index++)
            {
                resolvedSlots[index] = default(RecipeIconSlot);
            }

            if (HasAppliedOrder(resolvedCount))
            {
                return true;
            }

            if (!panel.TryApplyRecipeIcons(resolvedSlots, resolvedCount))
            {
                Disable("apply");
                return false;
            }

            appliedCount = resolvedCount;
            for (var index = 0; index < appliedRecipeIds.Length; index++)
            {
                appliedRecipeIds[index] = index < resolvedCount
                    ? resolvedSlots[index].RecipeId
                    : 0;
            }

            diagnostics.Write(
                TrackerDiagnosticLevel.Debug,
                "recipe-icon-slots action=refresh order=" + FormatAppliedOrder());
            return true;
        }

        public void Dispose()
        {
            if (released)
            {
                return;
            }

            released = true;
            available = false;
            ReleaseSlots();
            diagnostics.Write(
                TrackerDiagnosticLevel.Debug,
                "recipe-icon-slots action=release");
        }

        private bool HasAppliedOrder(int resolvedCount)
        {
            if (appliedCount != resolvedCount)
            {
                return false;
            }

            for (var index = 0; index < resolvedCount; index++)
            {
                if (appliedRecipeIds[index] != resolvedSlots[index].RecipeId)
                {
                    return false;
                }
            }

            return true;
        }

        private string FormatAppliedOrder()
        {
            var order = string.Empty;
            for (var index = 0; index < appliedCount; index++)
            {
                if (index != 0)
                {
                    order += ",";
                }

                order += appliedRecipeIds[index];
            }

            return "[" + order + "]";
        }

        private void ReportInvalidOnce(int recipeId)
        {
            if (!reportedInvalidRecipeIds.Add(recipeId))
            {
                return;
            }

            diagnostics.Write(
                TrackerDiagnosticLevel.Debug,
                "recipe-icon-slots action=remove-unavailable recipeId=" + recipeId +
                " reason=missing-recipe-or-icon");
        }

        private void Disable(string reason)
        {
            if (!available)
            {
                return;
            }

            available = false;
            ReleaseSlots();
            diagnostics.Write(
                TrackerDiagnosticLevel.Debug,
                "recipe-icon-slots action=disable reason=" + reason);
        }

        private void ReleaseSlots()
        {
            if (slotsReleased)
            {
                return;
            }

            slotsReleased = true;
            panel.ReleaseRecipeIcons();
        }
    }
}
