namespace DSPRecipeTracker
{
    internal sealed class UnityRecipeIconResolver : IRecipeIconResolver
    {
        public bool TryResolve(int recipeId, out RecipeIconHandle icon)
        {
            icon = default(RecipeIconHandle);
            try
            {
                var recipe = LDB.recipes.Select(recipeId);
                if (ReferenceEquals(recipe, null))
                {
                    return false;
                }

                var nativeIcon = recipe.iconSprite;
                if (ReferenceEquals(nativeIcon, null))
                {
                    return false;
                }

                icon = new RecipeIconHandle(nativeIcon);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
