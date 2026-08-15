#if NET8_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace DSPRecipeTracker
{
    internal enum IngredientValueTreatment
    {
        Insufficient,
        Sufficient
    }

    internal readonly struct IngredientRowView
    {
        public IngredientRowView(
            int itemId,
            PresentationIconHandle icon,
            string comparisonText,
            IngredientValueTreatment treatment)
        {
            ItemId = itemId;
            Icon = icon;
            ComparisonText = comparisonText;
            Treatment = treatment;
        }

        public int ItemId { get; }

        public PresentationIconHandle Icon { get; }

        public string ComparisonText { get; }

        public IngredientValueTreatment Treatment { get; }
    }

    internal sealed class RecipeRowView
    {
        private readonly ReadOnlyCollection<IngredientRowView> ingredients;

        public RecipeRowView(
            int recipeId,
            PresentationIconHandle productIcon,
            IngredientRowView[] ingredients,
            string machineWarning)
        {
            RecipeId = recipeId;
            ProductIcon = productIcon;
            this.ingredients = Array.AsReadOnly(
                ingredients ?? throw new ArgumentNullException(nameof(ingredients)));
            MachineWarning = machineWarning;
        }

        public int RecipeId { get; }

        public PresentationIconHandle ProductIcon { get; }

        public IReadOnlyList<IngredientRowView> Ingredients => ingredients;

        public string MachineWarning { get; }
    }

    internal enum RecipeRowUiResourceClass
    {
        None,
        PanelHost,
        NativeFont,
        RowContainer,
        ProductIcon,
        IngredientIcon,
        Text
    }

    internal readonly struct RecipeRowUiFailure
    {
        public RecipeRowUiFailure(int recipeId, RecipeRowUiResourceClass resourceClass)
        {
            RecipeId = recipeId;
            ResourceClass = resourceClass;
        }

        public int RecipeId { get; }

        public RecipeRowUiResourceClass ResourceClass { get; }
    }

    internal interface IRecipeRowUiAdapter
    {
        bool TryInitialize(out RecipeRowUiFailure failure);

        bool TryApplyRow(
            int rowIndex,
            RecipeRowView row,
            out RecipeRowUiFailure failure);

        bool TryHideRow(int rowIndex);

        void Release();
    }

    internal static class RecipeRowLayout
    {
        public const float HeaderHeight = 18f;
        public const float RowHeight = 90f;
        public const float FirstRowTop = 24f;
        public const float RowSpacing = 90f;
        public const float ContentHeight = 60f;
        public const float ProductLeft = 12f;
        public const float ProductTop = 1f;
        public const float ProductSize = 42f;
        public const float ProductLabelLeft = 12f;
        public const float ProductLabelTop = 60f;
        public const float ProductLabelWidth = 336f;
        public const float ProductLabelHeight = 30f;
        public const float SeparatorLeft = 70f;
        public const float IngredientFirstLeft = 76f;
        public const float IngredientCellWidth = 44f;
        public const float IngredientCellSpacing = 46f;
        public const float IngredientIconSize = 34f;

        public static float RowTop(int rowIndex)
        {
            return FirstRowTop + (RowSpacing * rowIndex);
        }

        public static float IngredientLeft(int ingredientIndex)
        {
            return IngredientFirstLeft + (IngredientCellSpacing * ingredientIndex);
        }
    }

    internal sealed class RecipeRowPresentation : IDisposable
    {
        private readonly IRecipeRowUiAdapter adapter;
        private readonly ITrackerDiagnosticSink diagnostics;
        private readonly HashSet<string> reportedFailures =
            new HashSet<string>(StringComparer.Ordinal);
        private RecipePresentationFrame appliedFrame;
        private RecipePresentationFrame observedFrame;
        private bool available;
        private bool released;

        public RecipeRowPresentation(
            IRecipeRowUiAdapter adapter,
            ITrackerDiagnosticSink diagnostics)
        {
            this.adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            this.diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        }

        public bool IsAvailable => available;

        public bool TryInitialize()
        {
            if (released || available)
            {
                return false;
            }

            RecipeRowUiFailure failure;
            try
            {
                if (!adapter.TryInitialize(out failure))
                {
                    return DisableDuringInitialization(failure);
                }
            }
            catch
            {
                return DisableDuringInitialization(new RecipeRowUiFailure(
                    0,
                    RecipeRowUiResourceClass.RowContainer));
            }

            available = true;
            diagnostics.Write(TrackerDiagnosticLevel.Debug, "recipe-rows action=initialize");
            return true;
        }

        public bool TryApplyFrame(RecipePresentationFrame frame)
        {
            if (!available || frame == null)
            {
                return false;
            }

            if (appliedFrame != null && appliedFrame.Equals(frame))
            {
                return true;
            }

            var changed = observedFrame == null || !observedFrame.Equals(frame);
            observedFrame = frame;

            if (frame.Rows.Count > PinnedRecipeState.Capacity)
            {
                appliedFrame = null;
                ReportFailure(new RecipeRowUiFailure(0, RecipeRowUiResourceClass.RowContainer));
                if (changed)
                {
                    ReportFrame(frame, 0, 0, 0);
                }

                return false;
            }

            var allRowsApplied = true;
            var appliedRows = 0;
            var sufficientCount = 0;
            var insufficientCount = 0;

            for (var rowIndex = 0; rowIndex < frame.Rows.Count; rowIndex++)
            {
                RecipeRowView view;
                RecipeRowUiFailure failure;
                if (!TryComposeRow(frame.Rows[rowIndex], out view, out failure))
                {
                    allRowsApplied = false;
                    TryHideRow(rowIndex);
                    ReportFailure(failure);
                    continue;
                }

                try
                {
                    if (!adapter.TryApplyRow(rowIndex, view, out failure))
                    {
                        allRowsApplied = false;
                        TryHideRow(rowIndex);
                        ReportFailure(failure);
                        continue;
                    }
                }
                catch
                {
                    allRowsApplied = false;
                    TryHideRow(rowIndex);
                    ReportFailure(new RecipeRowUiFailure(
                        view.RecipeId,
                        RecipeRowUiResourceClass.RowContainer));
                    continue;
                }

                appliedRows++;
                for (var ingredientIndex = 0;
                    ingredientIndex < view.Ingredients.Count;
                    ingredientIndex++)
                {
                    if (view.Ingredients[ingredientIndex].Treatment ==
                        IngredientValueTreatment.Sufficient)
                    {
                        sufficientCount++;
                    }
                    else
                    {
                        insufficientCount++;
                    }
                }
            }

            for (var rowIndex = frame.Rows.Count;
                rowIndex < PinnedRecipeState.Capacity;
                rowIndex++)
            {
                if (!TryHideRow(rowIndex))
                {
                    allRowsApplied = false;
                    ReportFailure(new RecipeRowUiFailure(
                        0,
                        RecipeRowUiResourceClass.RowContainer));
                }
            }

            if (allRowsApplied)
            {
                appliedFrame = frame;
            }
            else
            {
                appliedFrame = null;
            }

            if (changed)
            {
                ReportFrame(frame, appliedRows, sufficientCount, insufficientCount);
            }

            return allRowsApplied;
        }

        public void Dispose()
        {
            if (released)
            {
                return;
            }

            released = true;
            available = false;
            try
            {
                adapter.Release();
            }
            catch
            {
            }

            diagnostics.Write(TrackerDiagnosticLevel.Debug, "recipe-rows action=release");
        }

        private static bool TryComposeRow(
            RecipePresentationRow row,
            out RecipeRowView view,
            out RecipeRowUiFailure failure)
        {
            view = null;
            var recipeId = row == null ? 0 : row.RecipeId;
            if (row == null || recipeId <= 0)
            {
                failure = new RecipeRowUiFailure(recipeId, RecipeRowUiResourceClass.RowContainer);
                return false;
            }

            if (!row.RecipeIcon.IsAvailable)
            {
                failure = new RecipeRowUiFailure(recipeId, RecipeRowUiResourceClass.ProductIcon);
                return false;
            }

            if (row.Ingredients == null ||
                row.Ingredients.Count < RecipePresentationModel.MinimumIngredientCount ||
                row.Ingredients.Count > RecipePresentationModel.MaximumIngredientCount)
            {
                failure = new RecipeRowUiFailure(recipeId, RecipeRowUiResourceClass.RowContainer);
                return false;
            }

            var ingredients = new IngredientRowView[row.Ingredients.Count];
            for (var index = 0; index < row.Ingredients.Count; index++)
            {
                var ingredient = row.Ingredients[index];
                if (ingredient.ItemId <= 0 ||
                    ingredient.RequiredCount <= 0 ||
                    ingredient.CurrentCount < 0)
                {
                    failure = new RecipeRowUiFailure(recipeId, RecipeRowUiResourceClass.Text);
                    return false;
                }

                if (!ingredient.Icon.IsAvailable)
                {
                    failure = new RecipeRowUiFailure(recipeId, RecipeRowUiResourceClass.IngredientIcon);
                    return false;
                }

                ingredients[index] = new IngredientRowView(
                    ingredient.ItemId,
                    ingredient.Icon,
                    ingredient.CurrentCount.ToString(CultureInfo.InvariantCulture) + " / " +
                        ingredient.RequiredCount.ToString(CultureInfo.InvariantCulture),
                    ingredient.IsSufficient
                        ? IngredientValueTreatment.Sufficient
                        : IngredientValueTreatment.Insufficient);
            }

            view = new RecipeRowView(
                recipeId,
                row.RecipeIcon,
                ingredients,
                FormatMachineWarning(row.MachineWarning));
            failure = default(RecipeRowUiFailure);
            return true;
        }

        internal static string FormatMachineWarning(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var words = value.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", words);
        }

        private bool DisableDuringInitialization(RecipeRowUiFailure failure)
        {
            released = true;
            available = false;
            try
            {
                adapter.Release();
            }
            catch
            {
            }

            ReportFailure(failure);
            diagnostics.Write(TrackerDiagnosticLevel.Debug, "recipe-rows action=disable");
            return false;
        }

        private bool TryHideRow(int rowIndex)
        {
            try
            {
                return adapter.TryHideRow(rowIndex);
            }
            catch
            {
                return false;
            }
        }

        private void ReportFailure(RecipeRowUiFailure failure)
        {
            var key = failure.RecipeId + ":" + failure.ResourceClass;
            if (!reportedFailures.Add(key))
            {
                return;
            }

            diagnostics.Write(
                TrackerDiagnosticLevel.Debug,
                "recipe-rows action=suppress recipeId=" + failure.RecipeId +
                " resource=" + FormatResource(failure.ResourceClass));
        }

        private void ReportFrame(
            RecipePresentationFrame frame,
            int appliedRows,
            int sufficientCount,
            int insufficientCount)
        {
            var message = new StringBuilder("recipe-rows action=refresh rows=");
            message.Append(frame.Rows.Count);
            message.Append(" applied=");
            message.Append(appliedRows);
            message.Append(" recipes=[");
            for (var index = 0; index < frame.Rows.Count; index++)
            {
                if (index != 0)
                {
                    message.Append(',');
                }

                message.Append(frame.Rows[index].RecipeId);
                message.Append(':');
                message.Append(frame.Rows[index].Ingredients.Count);
            }

            message.Append("] sufficient=");
            message.Append(sufficientCount);
            message.Append(" insufficient=");
            message.Append(insufficientCount);
            diagnostics.Write(TrackerDiagnosticLevel.Debug, message.ToString());
        }

        private static string FormatResource(RecipeRowUiResourceClass resourceClass)
        {
            switch (resourceClass)
            {
                case RecipeRowUiResourceClass.PanelHost:
                    return "panel-host";
                case RecipeRowUiResourceClass.NativeFont:
                    return "native-font";
                case RecipeRowUiResourceClass.RowContainer:
                    return "row-container";
                case RecipeRowUiResourceClass.ProductIcon:
                    return "product-icon";
                case RecipeRowUiResourceClass.IngredientIcon:
                    return "ingredient-icon";
                case RecipeRowUiResourceClass.Text:
                    return "text";
                default:
                    return "unknown";
            }
        }
    }
}
