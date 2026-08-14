namespace DSPRecipeTracker
{
    internal static class VisibilityPolicy
    {
        public static bool IsVisible(
            bool hasRows,
            bool manualRequested,
            bool majorInterfaceActive)
        {
            return hasRows && manualRequested && !majorInterfaceActive;
        }
    }
}
