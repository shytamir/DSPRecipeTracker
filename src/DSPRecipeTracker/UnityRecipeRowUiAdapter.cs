using UnityEngine;
using UnityEngine.UI;

namespace DSPRecipeTracker
{
    internal sealed class UnityRecipeRowUiAdapter : IRecipeRowUiAdapter
    {
        private const int IngredientFontSize = 11;
        private const int WarningFontSize = 9;
        private static readonly Color SufficientColor =
            new Color(0.404f, 0.647f, 0.486f, 1f);
        private static readonly Color InsufficientColor =
            new Color(0.561f, 0.208f, 0.208f, 1f);

        private readonly UnityTrackerPanelAdapter panel;
        private readonly Font nativeFont;
        private readonly GameObject[] rowObjects =
            new GameObject[PinnedRecipeState.Capacity];
        private readonly Image[] productImages =
            new Image[PinnedRecipeState.Capacity];
        private readonly GameObject[,] ingredientObjects =
            new GameObject[PinnedRecipeState.Capacity, RecipePresentationModel.MaximumIngredientCount];
        private readonly Image[,] ingredientImages =
            new Image[PinnedRecipeState.Capacity, RecipePresentationModel.MaximumIngredientCount];
        private readonly Text[,] ingredientTexts =
            new Text[PinnedRecipeState.Capacity, RecipePresentationModel.MaximumIngredientCount];
        private readonly Text[] warningTexts =
            new Text[PinnedRecipeState.Capacity];
        private bool initialized;
        private bool released;

        public UnityRecipeRowUiAdapter(
            UnityTrackerPanelAdapter panel,
            Font nativeFont)
        {
            this.panel = panel;
            this.nativeFont = nativeFont;
        }

        public bool TryInitialize(out RecipeRowUiFailure failure)
        {
            failure = default(RecipeRowUiFailure);
            if (released || initialized)
            {
                failure = new RecipeRowUiFailure(0, RecipeRowUiResourceClass.RowContainer);
                return false;
            }

            var parent = ReferenceEquals(panel, null) ? null : panel.PanelTransform;
            if (ReferenceEquals(parent, null))
            {
                failure = new RecipeRowUiFailure(0, RecipeRowUiResourceClass.PanelHost);
                return false;
            }

            if (ReferenceEquals(nativeFont, null))
            {
                failure = new RecipeRowUiFailure(0, RecipeRowUiResourceClass.NativeFont);
                return false;
            }

            try
            {
                for (var rowIndex = 0;
                    rowIndex < PinnedRecipeState.Capacity;
                    rowIndex++)
                {
                    if (!TryCreateRow(parent, rowIndex))
                    {
                        failure = new RecipeRowUiFailure(0, RecipeRowUiResourceClass.RowContainer);
                        Release();
                        return false;
                    }
                }
            }
            catch
            {
                failure = new RecipeRowUiFailure(0, RecipeRowUiResourceClass.RowContainer);
                Release();
                return false;
            }

            initialized = true;
            return true;
        }

        public bool TryApplyRow(
            int rowIndex,
            RecipeRowView row,
            out RecipeRowUiFailure failure)
        {
            failure = default(RecipeRowUiFailure);
            if (!initialized || released ||
                rowIndex < 0 || rowIndex >= PinnedRecipeState.Capacity ||
                row == null ||
                row.Ingredients.Count < RecipePresentationModel.MinimumIngredientCount ||
                row.Ingredients.Count > RecipePresentationModel.MaximumIngredientCount)
            {
                failure = new RecipeRowUiFailure(
                    row == null ? 0 : row.RecipeId,
                    RecipeRowUiResourceClass.RowContainer);
                return false;
            }

            var productSprite = row.ProductIcon.Value as Sprite;
            if (ReferenceEquals(productSprite, null))
            {
                failure = new RecipeRowUiFailure(
                    row.RecipeId,
                    RecipeRowUiResourceClass.ProductIcon);
                return false;
            }

            for (var ingredientIndex = 0;
                ingredientIndex < row.Ingredients.Count;
                ingredientIndex++)
            {
                if (!(row.Ingredients[ingredientIndex].Icon.Value is Sprite))
                {
                    failure = new RecipeRowUiFailure(
                        row.RecipeId,
                        RecipeRowUiResourceClass.IngredientIcon);
                    return false;
                }
            }

            productImages[rowIndex].sprite = productSprite;
            for (var ingredientIndex = 0;
                ingredientIndex < RecipePresentationModel.MaximumIngredientCount;
                ingredientIndex++)
            {
                var active = ingredientIndex < row.Ingredients.Count;
                var ingredientObject = ingredientObjects[rowIndex, ingredientIndex];
                if (active)
                {
                    var ingredient = row.Ingredients[ingredientIndex];
                    ingredientImages[rowIndex, ingredientIndex].sprite =
                        (Sprite)ingredient.Icon.Value;
                    var text = ingredientTexts[rowIndex, ingredientIndex];
                    text.text = ingredient.ComparisonText;
                    text.color = ingredient.Treatment == IngredientValueTreatment.Sufficient
                        ? SufficientColor
                        : InsufficientColor;
                }

                ingredientObject.SetActive(active);
            }

            var warningText = warningTexts[rowIndex];
            var hasWarning = !string.IsNullOrWhiteSpace(row.MachineWarning);
            if (hasWarning)
            {
                warningText.text = row.MachineWarning;
                warningText.color = InsufficientColor;
            }

            warningText.gameObject.SetActive(hasWarning);
            rowObjects[rowIndex].SetActive(true);
            return true;
        }

        public bool TryHideRow(int rowIndex)
        {
            if (!initialized || released ||
                rowIndex < 0 || rowIndex >= PinnedRecipeState.Capacity ||
                ReferenceEquals(rowObjects[rowIndex], null))
            {
                return false;
            }

            rowObjects[rowIndex].SetActive(false);
            return true;
        }

        public void Release()
        {
            if (released)
            {
                return;
            }

            released = true;
            initialized = false;
            for (var rowIndex = 0;
                rowIndex < PinnedRecipeState.Capacity;
                rowIndex++)
            {
                var ownedRow = rowObjects[rowIndex];
                rowObjects[rowIndex] = null;
                productImages[rowIndex] = null;
                warningTexts[rowIndex] = null;
                for (var ingredientIndex = 0;
                    ingredientIndex < RecipePresentationModel.MaximumIngredientCount;
                    ingredientIndex++)
                {
                    ingredientObjects[rowIndex, ingredientIndex] = null;
                    ingredientImages[rowIndex, ingredientIndex] = null;
                    ingredientTexts[rowIndex, ingredientIndex] = null;
                }

                if (!ReferenceEquals(ownedRow, null))
                {
                    Object.Destroy(ownedRow);
                }
            }
        }

        private bool TryCreateRow(RectTransform parent, int rowIndex)
        {
            var rowObject = new GameObject("Recipe Row " + (rowIndex + 1));
            var rowTransform = (RectTransform)rowObject.AddComponent(typeof(RectTransform));
            if (ReferenceEquals(rowTransform, null))
            {
                return false;
            }

            rowTransform.SetParent(parent, false);
            ConfigureRect(
                rowTransform,
                0f,
                RecipeRowLayout.RowTop(rowIndex),
                PanelGeometry.FixedWidth,
                RecipeRowLayout.RowHeight);
            rowObjects[rowIndex] = rowObject;

            var productObject = new GameObject("Product Icon");
            var productTransform =
                (RectTransform)productObject.AddComponent(typeof(RectTransform));
            var productImage = (Image)productObject.AddComponent(typeof(Image));
            if (ReferenceEquals(productTransform, null) || ReferenceEquals(productImage, null))
            {
                return false;
            }

            productTransform.SetParent(rowTransform, false);
            ConfigureRect(
                productTransform,
                RecipeRowLayout.ProductLeft,
                RecipeRowLayout.ProductTop,
                RecipeRowLayout.ProductSize,
                RecipeRowLayout.ProductSize);
            productImage.raycastTarget = false;
            productImages[rowIndex] = productImage;

            for (var ingredientIndex = 0;
                ingredientIndex < RecipePresentationModel.MaximumIngredientCount;
                ingredientIndex++)
            {
                if (!TryCreateIngredientCell(rowTransform, rowIndex, ingredientIndex))
                {
                    return false;
                }
            }

            var warningObject = new GameObject("Machine Warning");
            var warningTransform =
                (RectTransform)warningObject.AddComponent(typeof(RectTransform));
            var warningText = (Text)warningObject.AddComponent(typeof(Text));
            if (ReferenceEquals(warningTransform, null) || ReferenceEquals(warningText, null))
            {
                return false;
            }

            warningTransform.SetParent(rowTransform, false);
            ConfigureRect(warningTransform, 4f, 54f, 62f, 14f);
            ConfigureText(warningText, WarningFontSize);
            warningObject.SetActive(false);
            warningTexts[rowIndex] = warningText;
            rowObject.SetActive(false);
            return true;
        }

        private bool TryCreateIngredientCell(
            RectTransform rowTransform,
            int rowIndex,
            int ingredientIndex)
        {
            var cellObject = new GameObject("Ingredient " + (ingredientIndex + 1));
            var cellTransform = (RectTransform)cellObject.AddComponent(typeof(RectTransform));
            if (ReferenceEquals(cellTransform, null))
            {
                return false;
            }

            cellTransform.SetParent(rowTransform, false);
            ConfigureRect(
                cellTransform,
                RecipeRowLayout.IngredientLeft(ingredientIndex),
                0f,
                RecipeRowLayout.IngredientCellWidth,
                RecipeRowLayout.RowHeight);

            var iconObject = new GameObject("Icon");
            var iconTransform = (RectTransform)iconObject.AddComponent(typeof(RectTransform));
            var iconImage = (Image)iconObject.AddComponent(typeof(Image));
            if (ReferenceEquals(iconTransform, null) || ReferenceEquals(iconImage, null))
            {
                return false;
            }

            iconTransform.SetParent(cellTransform, false);
            ConfigureRect(iconTransform, 5f, 3f, RecipeRowLayout.IngredientIconSize, RecipeRowLayout.IngredientIconSize);
            iconImage.raycastTarget = false;

            var textObject = new GameObject("Count");
            var textTransform = (RectTransform)textObject.AddComponent(typeof(RectTransform));
            var countText = (Text)textObject.AddComponent(typeof(Text));
            if (ReferenceEquals(textTransform, null) || ReferenceEquals(countText, null))
            {
                return false;
            }

            textTransform.SetParent(cellTransform, false);
            ConfigureRect(textTransform, 0f, 41f, RecipeRowLayout.IngredientCellWidth, 18f);
            ConfigureText(countText, IngredientFontSize);

            ingredientObjects[rowIndex, ingredientIndex] = cellObject;
            ingredientImages[rowIndex, ingredientIndex] = iconImage;
            ingredientTexts[rowIndex, ingredientIndex] = countText;
            cellObject.SetActive(false);
            return true;
        }

        private void ConfigureText(Text text, int fontSize)
        {
            text.font = nativeFont;
            text.fontSize = fontSize;
            text.raycastTarget = false;
        }

        private static void ConfigureRect(
            RectTransform transform,
            float left,
            float top,
            float width,
            float height)
        {
            var topLeft = new Vector2(0f, 1f);
            transform.anchorMin = topLeft;
            transform.anchorMax = topLeft;
            transform.pivot = topLeft;
            transform.anchoredPosition = new Vector2(left, -top);
            transform.sizeDelta = new Vector2(width, height);
        }
    }
}
