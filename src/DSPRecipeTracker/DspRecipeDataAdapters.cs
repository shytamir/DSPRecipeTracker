using System;

namespace DSPRecipeTracker
{
    internal sealed class DspRecipeDataAdapter : IRecipeDataAdapter
    {
        private RecipeProtoSet recipes;
        private ItemProtoSet items;
        private bool released;

        public bool TryRefresh()
        {
            if (released)
            {
                return false;
            }

            try
            {
                recipes = LDB.recipes;
                items = LDB.items;
                return !ReferenceEquals(recipes, null) && !ReferenceEquals(items, null);
            }
            catch
            {
                recipes = null;
                items = null;
                return false;
            }
        }

        public RecipeDataReadResult Read(int recipeId)
        {
            if (released || ReferenceEquals(recipes, null) || ReferenceEquals(items, null))
            {
                return RecipeDataReadResult.TemporarilyUnavailable(
                    RecipeDataFailureReason.AdapterUnavailable);
            }

            try
            {
                var recipe = recipes.Select(recipeId);
                if (ReferenceEquals(recipe, null))
                {
                    return RecipeDataReadResult.InvalidRecipe(RecipeDataFailureReason.MissingRecipe);
                }

                if (recipe.ID != recipeId)
                {
                    return RecipeDataReadResult.InvalidRecipe(
                        RecipeDataFailureReason.RecipeIdentityMismatch);
                }

                var recipeIcon = recipe.iconSprite;
                if (ReferenceEquals(recipeIcon, null))
                {
                    return RecipeDataReadResult.InvalidRecipe(
                        RecipeDataFailureReason.MissingRecipeIcon);
                }

                var ingredientIds = recipe.Items;
                var requiredCounts = recipe.ItemCounts;
                if (ingredientIds == null || requiredCounts == null ||
                    ingredientIds.Length == 0 || ingredientIds.Length != requiredCounts.Length)
                {
                    return RecipeDataReadResult.InvalidRecipe(
                        RecipeDataFailureReason.InvalidIngredientShape);
                }

                if (ingredientIds.Length > RecipePresentationModel.MaximumIngredientCount)
                {
                    return RecipeDataReadResult.TemporarilyUnavailable(
                        RecipeDataFailureReason.UnsupportedIngredientCount);
                }

                var ingredientIcons = new PresentationIconHandle[ingredientIds.Length];
                for (var index = 0; index < ingredientIds.Length; index++)
                {
                    var itemId = ingredientIds[index];
                    if (itemId <= 0)
                    {
                        return RecipeDataReadResult.InvalidRecipe(
                            RecipeDataFailureReason.InvalidIngredientId);
                    }

                    for (var earlierIndex = 0; earlierIndex < index; earlierIndex++)
                    {
                        if (ingredientIds[earlierIndex] == itemId)
                        {
                            return RecipeDataReadResult.InvalidRecipe(
                                RecipeDataFailureReason.DuplicateIngredientId);
                        }
                    }

                    if (requiredCounts[index] <= 0)
                    {
                        return RecipeDataReadResult.InvalidRecipe(
                            RecipeDataFailureReason.InvalidRequiredCount);
                    }

                    var item = items.Select(itemId);
                    if (ReferenceEquals(item, null))
                    {
                        return RecipeDataReadResult.InvalidItem(
                            RecipeDataFailureReason.MissingItem,
                            itemId);
                    }

                    if (item.ID != itemId)
                    {
                        return RecipeDataReadResult.InvalidItem(
                            RecipeDataFailureReason.ItemIdentityMismatch,
                            itemId);
                    }

                    var itemIcon = item.iconSprite;
                    if (ReferenceEquals(itemIcon, null))
                    {
                        return RecipeDataReadResult.InvalidItem(
                            RecipeDataFailureReason.MissingItemIcon,
                            itemId);
                    }

                    ingredientIcons[index] = new PresentationIconHandle(itemIcon);
                }

                var isHandCraftable = recipe.Handcraft;
                var productionCategory = isHandCraftable ? null : recipe.madeFromString;
                if (!isHandCraftable && string.IsNullOrWhiteSpace(productionCategory))
                {
                    return RecipeDataReadResult.TemporarilyUnavailable(
                        RecipeDataFailureReason.MissingProductionCategory);
                }

                return RecipeDataReadResult.Success(new ResolvedRecipeData(
                    recipeId,
                    new PresentationIconHandle(recipeIcon),
                    ingredientIds,
                    ingredientIcons,
                    requiredCounts,
                    isHandCraftable,
                    productionCategory));
            }
            catch
            {
                return RecipeDataReadResult.TemporarilyUnavailable(
                    RecipeDataFailureReason.ReadFailure);
            }
        }

        public void Release()
        {
            if (released)
            {
                return;
            }

            released = true;
            recipes = null;
            items = null;
        }
    }

    internal sealed class DspInventoryDataAdapter : IInventoryDataAdapter
    {
        private StorageComponent package;
        private bool released;

        public bool TryRefresh()
        {
            if (released)
            {
                return false;
            }

            try
            {
                var player = GameMain.mainPlayer;
                package = ReferenceEquals(player, null) ? null : player.package;
                return !ReferenceEquals(package, null);
            }
            catch
            {
                package = null;
                return false;
            }
        }

        public bool TryGetItemCount(int itemId, out int count)
        {
            count = 0;
            if (released || itemId <= 0 || ReferenceEquals(package, null))
            {
                return false;
            }

            try
            {
                count = package.GetItemCount(itemId);
                return count >= 0;
            }
            catch
            {
                count = 0;
                return false;
            }
        }

        public void Release()
        {
            if (released)
            {
                return;
            }

            released = true;
            package = null;
        }
    }
}
