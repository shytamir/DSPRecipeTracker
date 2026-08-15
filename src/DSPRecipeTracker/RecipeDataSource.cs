#if NET8_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DSPRecipeTracker
{
    internal enum RecipeDataReadKind
    {
        Success,
        InvalidRecipe,
        InvalidItem,
        TemporarilyUnavailable
    }

    internal enum RecipeDataFailureReason
    {
        None,
        AdapterUnavailable,
        ReadFailure,
        MissingRecipe,
        RecipeIdentityMismatch,
        MissingRecipeIcon,
        InvalidIngredientShape,
        UnsupportedIngredientCount,
        InvalidIngredientId,
        DuplicateIngredientId,
        InvalidRequiredCount,
        MissingItem,
        ItemIdentityMismatch,
        MissingItemIcon,
        MissingProductionCategory,
        InventoryUnavailable,
        InventoryReadFailure
    }

    internal sealed class ResolvedRecipeData
    {
        private readonly ReadOnlyCollection<int> ingredientIds;
        private readonly ReadOnlyCollection<PresentationIconHandle> ingredientIcons;
        private readonly ReadOnlyCollection<int> requiredCounts;

        public ResolvedRecipeData(
            int recipeId,
            PresentationIconHandle recipeIcon,
            int[] ingredientIds,
            PresentationIconHandle[] ingredientIcons,
            int[] requiredCounts,
            bool isHandCraftable,
            string productionCategory)
        {
            RecipeId = recipeId;
            RecipeIcon = recipeIcon;
            this.ingredientIds = Array.AsReadOnly(
                (int[])(ingredientIds ?? throw new ArgumentNullException(nameof(ingredientIds))).Clone());
            this.ingredientIcons = Array.AsReadOnly(
                (PresentationIconHandle[])(ingredientIcons ?? throw new ArgumentNullException(nameof(ingredientIcons))).Clone());
            this.requiredCounts = Array.AsReadOnly(
                (int[])(requiredCounts ?? throw new ArgumentNullException(nameof(requiredCounts))).Clone());
            IsHandCraftable = isHandCraftable;
            ProductionCategory = productionCategory;
        }

        public int RecipeId { get; }

        public PresentationIconHandle RecipeIcon { get; }

        public IReadOnlyList<int> IngredientIds => ingredientIds;

        public IReadOnlyList<PresentationIconHandle> IngredientIcons => ingredientIcons;

        public IReadOnlyList<int> RequiredCounts => requiredCounts;

        public bool IsHandCraftable { get; }

        public string ProductionCategory { get; }
    }

    internal readonly struct RecipeDataReadResult
    {
        private RecipeDataReadResult(
            RecipeDataReadKind kind,
            RecipeDataFailureReason reason,
            ResolvedRecipeData data,
            int failedItemId)
        {
            Kind = kind;
            Reason = reason;
            Data = data;
            FailedItemId = failedItemId;
        }

        public RecipeDataReadKind Kind { get; }

        public RecipeDataFailureReason Reason { get; }

        public ResolvedRecipeData Data { get; }

        public int FailedItemId { get; }

        public static RecipeDataReadResult Success(ResolvedRecipeData data)
        {
            return new RecipeDataReadResult(
                RecipeDataReadKind.Success,
                RecipeDataFailureReason.None,
                data ?? throw new ArgumentNullException(nameof(data)),
                0);
        }

        public static RecipeDataReadResult InvalidRecipe(RecipeDataFailureReason reason)
        {
            return new RecipeDataReadResult(RecipeDataReadKind.InvalidRecipe, reason, null, 0);
        }

        public static RecipeDataReadResult InvalidItem(RecipeDataFailureReason reason, int itemId)
        {
            return new RecipeDataReadResult(RecipeDataReadKind.InvalidItem, reason, null, itemId);
        }

        public static RecipeDataReadResult TemporarilyUnavailable(
            RecipeDataFailureReason reason,
            int itemId = 0)
        {
            return new RecipeDataReadResult(
                RecipeDataReadKind.TemporarilyUnavailable,
                reason,
                null,
                itemId);
        }
    }

    internal interface IRecipeDataAdapter
    {
        bool TryRefresh();

        RecipeDataReadResult Read(int recipeId);

        void Release();
    }

    internal interface IInventoryDataAdapter
    {
        bool TryRefresh();

        bool TryGetItemCount(int itemId, out int count);

        void Release();
    }

    internal sealed class RecipePresentationInputCollection
    {
        private readonly ReadOnlyCollection<RecipePresentationInput> inputs;
        private readonly ReadOnlyCollection<int> suppressedRecipeIds;
        private readonly ReadOnlyCollection<int> removedRecipeIds;

        public RecipePresentationInputCollection(
            RecipePresentationInput[] inputs,
            int[] suppressedRecipeIds,
            int[] removedRecipeIds,
            bool isReleased)
        {
            this.inputs = Array.AsReadOnly(
                inputs ?? throw new ArgumentNullException(nameof(inputs)));
            this.suppressedRecipeIds = Array.AsReadOnly(
                suppressedRecipeIds ?? throw new ArgumentNullException(nameof(suppressedRecipeIds)));
            this.removedRecipeIds = Array.AsReadOnly(
                removedRecipeIds ?? throw new ArgumentNullException(nameof(removedRecipeIds)));
            IsReleased = isReleased;
        }

        public IReadOnlyList<RecipePresentationInput> Inputs => inputs;

        public IReadOnlyList<int> SuppressedRecipeIds => suppressedRecipeIds;

        public IReadOnlyList<int> RemovedRecipeIds => removedRecipeIds;

        public bool IsReleased { get; }
    }

    internal sealed class RecipePresentationInputSource : IDisposable
    {
        private readonly PinnedRecipeState pinnedRecipes;
        private readonly IRecipeDataAdapter recipeAdapter;
        private readonly IInventoryDataAdapter inventoryAdapter;
        private readonly ITrackerDiagnosticSink diagnostics;
        private readonly HashSet<string> reportedFailures = new HashSet<string>(StringComparer.Ordinal);
        private bool? recipeAdapterAvailable;
        private bool? inventoryAdapterAvailable;
        private bool released;

        public RecipePresentationInputSource(
            PinnedRecipeState pinnedRecipes,
            IRecipeDataAdapter recipeAdapter,
            IInventoryDataAdapter inventoryAdapter,
            ITrackerDiagnosticSink diagnostics)
        {
            this.pinnedRecipes = pinnedRecipes ?? throw new ArgumentNullException(nameof(pinnedRecipes));
            this.recipeAdapter = recipeAdapter ?? throw new ArgumentNullException(nameof(recipeAdapter));
            this.inventoryAdapter = inventoryAdapter ?? throw new ArgumentNullException(nameof(inventoryAdapter));
            this.diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        }

        public RecipePresentationInputCollection Collect()
        {
            if (released)
            {
                return EmptyCollection(true);
            }

            var recipeIds = SnapshotRecipeIds();
            if (recipeIds.Length == 0)
            {
                return EmptyCollection(false);
            }

            var recipeAvailable = TryRefreshRecipeAdapter();
            var inventoryAvailable = TryRefreshInventoryAdapter();
            ReportAvailability("recipe", recipeAvailable, ref recipeAdapterAvailable);
            ReportAvailability("inventory", inventoryAvailable, ref inventoryAdapterAvailable);

            if (!recipeAvailable || !inventoryAvailable)
            {
                var reason = recipeAvailable
                    ? RecipeDataFailureReason.InventoryUnavailable
                    : RecipeDataFailureReason.AdapterUnavailable;
                for (var index = 0; index < recipeIds.Length; index++)
                {
                    ReportFailure("suppress", recipeIds[index], 0, reason);
                }

                return new RecipePresentationInputCollection(
                    new RecipePresentationInput[0],
                    recipeIds,
                    new int[0],
                    false);
            }

            var inputs = new List<RecipePresentationInput>(recipeIds.Length);
            var suppressed = new List<int>();
            var removed = new List<int>();

            for (var index = 0; index < recipeIds.Length; index++)
            {
                var recipeId = recipeIds[index];
                var read = TryReadRecipe(recipeId);
                if (read.Kind == RecipeDataReadKind.InvalidRecipe ||
                    read.Kind == RecipeDataReadKind.InvalidItem)
                {
                    pinnedRecipes.RemoveUnavailable(recipeId);
                    removed.Add(recipeId);
                    ReportFailure("remove-invalid", recipeId, read.FailedItemId, read.Reason);
                    continue;
                }

                if (read.Kind != RecipeDataReadKind.Success || read.Data == null)
                {
                    suppressed.Add(recipeId);
                    ReportFailure("suppress", recipeId, read.FailedItemId, read.Reason);
                    continue;
                }

                var data = read.Data;
                var currentCounts = new int[data.IngredientIds.Count];
                var inventoryRead = true;
                var failedItemId = 0;
                for (var ingredientIndex = 0; ingredientIndex < data.IngredientIds.Count; ingredientIndex++)
                {
                    var itemId = data.IngredientIds[ingredientIndex];
                    try
                    {
                        if (!inventoryAdapter.TryGetItemCount(itemId, out var count) || count < 0)
                        {
                            inventoryRead = false;
                            failedItemId = itemId;
                            break;
                        }

                        currentCounts[ingredientIndex] = count;
                    }
                    catch
                    {
                        inventoryRead = false;
                        failedItemId = itemId;
                        break;
                    }
                }

                if (!inventoryRead)
                {
                    suppressed.Add(recipeId);
                    ReportFailure(
                        "suppress",
                        recipeId,
                        failedItemId,
                        RecipeDataFailureReason.InventoryReadFailure);
                    continue;
                }

                inputs.Add(new RecipePresentationInput(
                    data.RecipeId,
                    data.RecipeIcon,
                    data.IngredientIds,
                    data.IngredientIcons,
                    data.RequiredCounts,
                    currentCounts,
                    data.IsHandCraftable,
                    data.ProductionCategory));
            }

            return new RecipePresentationInputCollection(
                inputs.ToArray(),
                suppressed.ToArray(),
                removed.ToArray(),
                false);
        }

        public void Dispose()
        {
            if (released)
            {
                return;
            }

            released = true;
            try
            {
                recipeAdapter.Release();
            }
            catch
            {
            }

            try
            {
                inventoryAdapter.Release();
            }
            catch
            {
            }

            diagnostics.Write(TrackerDiagnosticLevel.Debug, "recipe-data action=release");
        }

        private int[] SnapshotRecipeIds()
        {
            var source = pinnedRecipes.RecipeIds;
            var result = new int[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                result[index] = source[index];
            }

            return result;
        }

        private bool TryRefreshRecipeAdapter()
        {
            try
            {
                return recipeAdapter.TryRefresh();
            }
            catch
            {
                return false;
            }
        }

        private bool TryRefreshInventoryAdapter()
        {
            try
            {
                return inventoryAdapter.TryRefresh();
            }
            catch
            {
                return false;
            }
        }

        private RecipeDataReadResult TryReadRecipe(int recipeId)
        {
            try
            {
                return recipeAdapter.Read(recipeId);
            }
            catch
            {
                return RecipeDataReadResult.TemporarilyUnavailable(
                    RecipeDataFailureReason.ReadFailure);
            }
        }

        private void ReportAvailability(string adapter, bool available, ref bool? previous)
        {
            if (previous.HasValue && previous.Value == available)
            {
                return;
            }

            previous = available;
            diagnostics.Write(
                TrackerDiagnosticLevel.Debug,
                "recipe-data adapter=" + adapter + " available=" +
                available.ToString().ToLowerInvariant());
        }

        private void ReportFailure(
            string action,
            int recipeId,
            int itemId,
            RecipeDataFailureReason reason)
        {
            var identity = itemId > 0 ? "item:" + itemId : "recipe:" + recipeId;
            if (!reportedFailures.Add(identity))
            {
                return;
            }

            var message = "recipe-data action=" + action + " recipeId=" + recipeId;
            if (itemId > 0)
            {
                message += " itemId=" + itemId;
            }

            diagnostics.Write(
                TrackerDiagnosticLevel.Debug,
                message + " reason=" + FormatReason(reason));
        }

        private static RecipePresentationInputCollection EmptyCollection(bool isReleased)
        {
            return new RecipePresentationInputCollection(
                new RecipePresentationInput[0],
                new int[0],
                new int[0],
                isReleased);
        }

        private static string FormatReason(RecipeDataFailureReason reason)
        {
            switch (reason)
            {
                case RecipeDataFailureReason.AdapterUnavailable:
                    return "recipe-adapter-unavailable";
                case RecipeDataFailureReason.ReadFailure:
                    return "recipe-read-failure";
                case RecipeDataFailureReason.MissingRecipe:
                    return "missing-recipe";
                case RecipeDataFailureReason.RecipeIdentityMismatch:
                    return "recipe-identity-mismatch";
                case RecipeDataFailureReason.MissingRecipeIcon:
                    return "missing-recipe-icon";
                case RecipeDataFailureReason.InvalidIngredientShape:
                    return "invalid-ingredient-shape";
                case RecipeDataFailureReason.UnsupportedIngredientCount:
                    return "unsupported-ingredient-count";
                case RecipeDataFailureReason.InvalidIngredientId:
                    return "invalid-ingredient-id";
                case RecipeDataFailureReason.DuplicateIngredientId:
                    return "duplicate-ingredient-id";
                case RecipeDataFailureReason.InvalidRequiredCount:
                    return "invalid-required-count";
                case RecipeDataFailureReason.MissingItem:
                    return "missing-item";
                case RecipeDataFailureReason.ItemIdentityMismatch:
                    return "item-identity-mismatch";
                case RecipeDataFailureReason.MissingItemIcon:
                    return "missing-item-icon";
                case RecipeDataFailureReason.MissingProductionCategory:
                    return "missing-production-category";
                case RecipeDataFailureReason.InventoryUnavailable:
                    return "inventory-unavailable";
                case RecipeDataFailureReason.InventoryReadFailure:
                    return "inventory-read-failure";
                default:
                    return "unknown";
            }
        }
    }
}
