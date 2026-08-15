#if NET8_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace DSPRecipeTracker
{
    internal readonly struct PresentationIconHandle : IEquatable<PresentationIconHandle>
    {
        public PresentationIconHandle(object value)
        {
            Value = value;
        }

        public object Value { get; }

        public bool IsAvailable => Value != null;

        public bool Equals(PresentationIconHandle other)
        {
            return ReferenceEquals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is PresentationIconHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : RuntimeHelpers.GetHashCode(Value);
        }
    }

    internal sealed class RecipePresentationInput
    {
        public RecipePresentationInput(
            int recipeId,
            PresentationIconHandle recipeIcon,
            IReadOnlyList<int> ingredientIds,
            IReadOnlyList<PresentationIconHandle> ingredientIcons,
            IReadOnlyList<int> requiredCounts,
            IReadOnlyList<int> currentCounts,
            bool isHandCraftable,
            string productionCategory)
        {
            RecipeId = recipeId;
            RecipeIcon = recipeIcon;
            IngredientIds = ingredientIds;
            IngredientIcons = ingredientIcons;
            RequiredCounts = requiredCounts;
            CurrentCounts = currentCounts;
            IsHandCraftable = isHandCraftable;
            ProductionCategory = productionCategory;
        }

        public int RecipeId { get; }

        public PresentationIconHandle RecipeIcon { get; }

        public IReadOnlyList<int> IngredientIds { get; }

        public IReadOnlyList<PresentationIconHandle> IngredientIcons { get; }

        public IReadOnlyList<int> RequiredCounts { get; }

        public IReadOnlyList<int> CurrentCounts { get; }

        public bool IsHandCraftable { get; }

        public string ProductionCategory { get; }
    }

    internal readonly struct IngredientPresentation : IEquatable<IngredientPresentation>
    {
        public IngredientPresentation(
            int itemId,
            PresentationIconHandle icon,
            int requiredCount,
            int currentCount)
        {
            ItemId = itemId;
            Icon = icon;
            RequiredCount = requiredCount;
            CurrentCount = currentCount;
        }

        public int ItemId { get; }

        public PresentationIconHandle Icon { get; }

        public int RequiredCount { get; }

        public int CurrentCount { get; }

        public bool IsSufficient => CurrentCount >= RequiredCount;

        public bool Equals(IngredientPresentation other)
        {
            return ItemId == other.ItemId &&
                Icon.Equals(other.Icon) &&
                RequiredCount == other.RequiredCount &&
                CurrentCount == other.CurrentCount;
        }

        public override bool Equals(object obj)
        {
            return obj is IngredientPresentation other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = ItemId;
                hash = (hash * 397) ^ Icon.GetHashCode();
                hash = (hash * 397) ^ RequiredCount;
                return (hash * 397) ^ CurrentCount;
            }
        }
    }

    internal sealed class RecipePresentationRow : IEquatable<RecipePresentationRow>
    {
        private readonly ReadOnlyCollection<IngredientPresentation> ingredients;

        public RecipePresentationRow(
            int recipeId,
            PresentationIconHandle recipeIcon,
            IngredientPresentation[] ingredients,
            string machineWarning)
        {
            RecipeId = recipeId;
            RecipeIcon = recipeIcon;
            this.ingredients = Array.AsReadOnly(
                ingredients ?? throw new ArgumentNullException(nameof(ingredients)));
            MachineWarning = machineWarning;
        }

        public int RecipeId { get; }

        public PresentationIconHandle RecipeIcon { get; }

        public IReadOnlyList<IngredientPresentation> Ingredients => ingredients;

        public string MachineWarning { get; }

        public bool Equals(RecipePresentationRow other)
        {
            if (ReferenceEquals(other, null) ||
                RecipeId != other.RecipeId ||
                !RecipeIcon.Equals(other.RecipeIcon) ||
                !string.Equals(MachineWarning, other.MachineWarning, StringComparison.Ordinal) ||
                ingredients.Count != other.ingredients.Count)
            {
                return false;
            }

            for (var index = 0; index < ingredients.Count; index++)
            {
                if (!ingredients[index].Equals(other.ingredients[index]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RecipePresentationRow);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = RecipeId;
                hash = (hash * 397) ^ RecipeIcon.GetHashCode();
                hash = (hash * 397) ^ (MachineWarning == null ? 0 : MachineWarning.GetHashCode());
                for (var index = 0; index < ingredients.Count; index++)
                {
                    hash = (hash * 397) ^ ingredients[index].GetHashCode();
                }

                return hash;
            }
        }
    }

    internal sealed class RecipePresentationFrame : IEquatable<RecipePresentationFrame>
    {
        private readonly ReadOnlyCollection<RecipePresentationRow> rows;

        public RecipePresentationFrame(RecipePresentationRow[] rows)
        {
            this.rows = Array.AsReadOnly(rows ?? throw new ArgumentNullException(nameof(rows)));
        }

        public IReadOnlyList<RecipePresentationRow> Rows => rows;

        public bool Equals(RecipePresentationFrame other)
        {
            if (ReferenceEquals(other, null) || rows.Count != other.rows.Count)
            {
                return false;
            }

            for (var index = 0; index < rows.Count; index++)
            {
                if (!rows[index].Equals(other.rows[index]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RecipePresentationFrame);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                for (var index = 0; index < rows.Count; index++)
                {
                    hash = (hash * 397) ^ rows[index].GetHashCode();
                }

                return hash;
            }
        }
    }

    internal enum RecipePresentationFailureReason
    {
        MissingInput,
        InvalidRecipeId,
        MissingRecipeIcon,
        InvalidIngredientShape,
        UnsupportedIngredientCount,
        InvalidIngredientId,
        DuplicateIngredientId,
        MissingIngredientIcon,
        InvalidRequiredCount,
        InvalidCurrentCount,
        MissingProductionCategory
    }

    internal readonly struct RecipePresentationFailure
    {
        public RecipePresentationFailure(int recipeId, RecipePresentationFailureReason reason)
        {
            RecipeId = recipeId;
            Reason = reason;
        }

        public int RecipeId { get; }

        public RecipePresentationFailureReason Reason { get; }
    }

    internal sealed class RecipePresentationBuildResult
    {
        private readonly ReadOnlyCollection<RecipePresentationFailure> failures;

        public RecipePresentationBuildResult(
            RecipePresentationFrame frame,
            RecipePresentationFailure[] failures,
            bool changed)
        {
            Frame = frame ?? throw new ArgumentNullException(nameof(frame));
            this.failures = Array.AsReadOnly(
                failures ?? throw new ArgumentNullException(nameof(failures)));
            Changed = changed;
        }

        public RecipePresentationFrame Frame { get; }

        public IReadOnlyList<RecipePresentationFailure> Failures => failures;

        public bool Changed { get; }
    }

    internal sealed class RecipePresentationModel
    {
        internal const int MinimumIngredientCount = 1;
        internal const int MaximumIngredientCount = 6;

        private readonly ITrackerDiagnosticSink diagnostics;
        private readonly HashSet<int> reportedInvalidRecipeIds = new HashSet<int>();
        private RecipePresentationFrame previousFrame;

        public RecipePresentationModel(ITrackerDiagnosticSink diagnostics)
        {
            this.diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        }

        public RecipePresentationBuildResult Build(IReadOnlyList<RecipePresentationInput> inputs)
        {
            var rows = new List<RecipePresentationRow>();
            var failures = new List<RecipePresentationFailure>();

            if (inputs == null)
            {
                AddFailure(failures, 0, RecipePresentationFailureReason.MissingInput);
            }
            else
            {
                for (var index = 0; index < inputs.Count; index++)
                {
                    RecipePresentationRow row;
                    RecipePresentationFailure failure;
                    if (TryBuildRow(inputs[index], out row, out failure))
                    {
                        rows.Add(row);
                    }
                    else
                    {
                        AddFailure(failures, failure.RecipeId, failure.Reason);
                    }
                }
            }

            var frame = new RecipePresentationFrame(rows.ToArray());
            var changed = previousFrame == null || !previousFrame.Equals(frame);
            previousFrame = frame;

            if (changed)
            {
                ReportFrame(frame);
            }

            return new RecipePresentationBuildResult(frame, failures.ToArray(), changed);
        }

        private bool TryBuildRow(
            RecipePresentationInput input,
            out RecipePresentationRow row,
            out RecipePresentationFailure failure)
        {
            row = null;
            if (input == null)
            {
                failure = new RecipePresentationFailure(0, RecipePresentationFailureReason.MissingInput);
                return false;
            }

            var recipeId = input.RecipeId;
            if (recipeId <= 0)
            {
                failure = new RecipePresentationFailure(recipeId, RecipePresentationFailureReason.InvalidRecipeId);
                return false;
            }

            if (!input.RecipeIcon.IsAvailable)
            {
                failure = new RecipePresentationFailure(recipeId, RecipePresentationFailureReason.MissingRecipeIcon);
                return false;
            }

            if (input.IngredientIds == null ||
                input.IngredientIcons == null ||
                input.RequiredCounts == null ||
                input.CurrentCounts == null)
            {
                failure = new RecipePresentationFailure(recipeId, RecipePresentationFailureReason.InvalidIngredientShape);
                return false;
            }

            var ingredientCount = input.IngredientIds.Count;
            if (input.IngredientIcons.Count != ingredientCount ||
                input.RequiredCounts.Count != ingredientCount ||
                input.CurrentCounts.Count != ingredientCount)
            {
                failure = new RecipePresentationFailure(recipeId, RecipePresentationFailureReason.InvalidIngredientShape);
                return false;
            }

            if (ingredientCount < MinimumIngredientCount || ingredientCount > MaximumIngredientCount)
            {
                failure = new RecipePresentationFailure(recipeId, RecipePresentationFailureReason.UnsupportedIngredientCount);
                return false;
            }

            if (!input.IsHandCraftable && string.IsNullOrWhiteSpace(input.ProductionCategory))
            {
                failure = new RecipePresentationFailure(recipeId, RecipePresentationFailureReason.MissingProductionCategory);
                return false;
            }

            var ingredients = new IngredientPresentation[ingredientCount];
            for (var index = 0; index < ingredientCount; index++)
            {
                var itemId = input.IngredientIds[index];
                if (itemId <= 0)
                {
                    failure = new RecipePresentationFailure(recipeId, RecipePresentationFailureReason.InvalidIngredientId);
                    return false;
                }

                for (var earlierIndex = 0; earlierIndex < index; earlierIndex++)
                {
                    if (input.IngredientIds[earlierIndex] == itemId)
                    {
                        failure = new RecipePresentationFailure(recipeId, RecipePresentationFailureReason.DuplicateIngredientId);
                        return false;
                    }
                }

                var icon = input.IngredientIcons[index];
                if (!icon.IsAvailable)
                {
                    failure = new RecipePresentationFailure(recipeId, RecipePresentationFailureReason.MissingIngredientIcon);
                    return false;
                }

                var requiredCount = input.RequiredCounts[index];
                if (requiredCount <= 0)
                {
                    failure = new RecipePresentationFailure(recipeId, RecipePresentationFailureReason.InvalidRequiredCount);
                    return false;
                }

                var currentCount = input.CurrentCounts[index];
                if (currentCount < 0)
                {
                    failure = new RecipePresentationFailure(recipeId, RecipePresentationFailureReason.InvalidCurrentCount);
                    return false;
                }

                ingredients[index] = new IngredientPresentation(
                    itemId,
                    icon,
                    requiredCount,
                    currentCount);
            }

            row = new RecipePresentationRow(
                recipeId,
                input.RecipeIcon,
                ingredients,
                input.IsHandCraftable ? null : input.ProductionCategory);
            failure = default(RecipePresentationFailure);
            return true;
        }

        private void AddFailure(
            List<RecipePresentationFailure> failures,
            int recipeId,
            RecipePresentationFailureReason reason)
        {
            failures.Add(new RecipePresentationFailure(recipeId, reason));
            if (reportedInvalidRecipeIds.Add(recipeId))
            {
                diagnostics.Write(
                    TrackerDiagnosticLevel.Debug,
                    "recipe-presentation action=invalid recipeId=" + recipeId +
                    " reason=" + FormatReason(reason));
            }
        }

        private void ReportFrame(RecipePresentationFrame frame)
        {
            var sufficientCount = 0;
            var insufficientCount = 0;
            var message = new StringBuilder("recipe-presentation action=refresh rows=");
            message.Append(frame.Rows.Count);
            message.Append(" recipes=[");

            var reportedRows = Math.Min(frame.Rows.Count, PinnedRecipeState.Capacity);
            for (var rowIndex = 0; rowIndex < reportedRows; rowIndex++)
            {
                if (rowIndex != 0)
                {
                    message.Append(',');
                }

                var row = frame.Rows[rowIndex];
                message.Append(row.RecipeId);
                message.Append(':');
                message.Append(row.Ingredients.Count);
            }

            message.Append(']');
            for (var rowIndex = 0; rowIndex < frame.Rows.Count; rowIndex++)
            {
                var ingredients = frame.Rows[rowIndex].Ingredients;
                for (var ingredientIndex = 0; ingredientIndex < ingredients.Count; ingredientIndex++)
                {
                    if (ingredients[ingredientIndex].IsSufficient)
                    {
                        sufficientCount++;
                    }
                    else
                    {
                        insufficientCount++;
                    }
                }
            }

            message.Append(" sufficient=");
            message.Append(sufficientCount);
            message.Append(" insufficient=");
            message.Append(insufficientCount);
            diagnostics.Write(TrackerDiagnosticLevel.Debug, message.ToString());
        }

        private static string FormatReason(RecipePresentationFailureReason reason)
        {
            switch (reason)
            {
                case RecipePresentationFailureReason.MissingInput:
                    return "missing-input";
                case RecipePresentationFailureReason.InvalidRecipeId:
                    return "invalid-recipe-id";
                case RecipePresentationFailureReason.MissingRecipeIcon:
                    return "missing-recipe-icon";
                case RecipePresentationFailureReason.InvalidIngredientShape:
                    return "invalid-ingredient-shape";
                case RecipePresentationFailureReason.UnsupportedIngredientCount:
                    return "unsupported-ingredient-count";
                case RecipePresentationFailureReason.InvalidIngredientId:
                    return "invalid-ingredient-id";
                case RecipePresentationFailureReason.DuplicateIngredientId:
                    return "duplicate-ingredient-id";
                case RecipePresentationFailureReason.MissingIngredientIcon:
                    return "missing-ingredient-icon";
                case RecipePresentationFailureReason.InvalidRequiredCount:
                    return "invalid-required-count";
                case RecipePresentationFailureReason.InvalidCurrentCount:
                    return "invalid-current-count";
                case RecipePresentationFailureReason.MissingProductionCategory:
                    return "missing-production-category";
                default:
                    return "unknown";
            }
        }
    }
}
