using UnityEngine;
using UnityEngine.UI;

namespace DSPRecipeTracker
{
    internal sealed class UnityRecipeRowUiAdapter : IRecipeRowUiAdapter
    {
        private const int HeaderFontSize = 12;
        private const int IngredientFontSize = 13;
        private const int WarningFontSize = 11;
        private static readonly Color HeaderColor =
            new Color(0.78f, 0.9f, 1f, 1f);
        private static readonly Color SufficientColor =
            new Color(0.35f, 0.9f, 0.55f, 1f);
        private static readonly Color InsufficientColor =
            new Color(1f, 0.38f, 0.32f, 1f);
        private static readonly Color MachineColor =
            new Color(0.95f, 0.72f, 0.3f, 1f);
        private static readonly Color SeparatorColor =
            new Color(0.12f, 0.68f, 0.82f, 0.45f);

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
        private GameObject semanticHeaderObject;
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
                if (!TryCreateSemanticHeader(parent))
                {
                    failure = new RecipeRowUiFailure(0, RecipeRowUiResourceClass.RowContainer);
                    Release();
                    return false;
                }

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

            var hasWarning = !string.IsNullOrWhiteSpace(row.MachineWarning);
            var warningText = warningTexts[rowIndex];
            if (hasWarning)
            {
                warningText.text = row.MachineWarning;
                warningText.color = MachineColor;
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
            var ownedHeader = semanticHeaderObject;
            semanticHeaderObject = null;
            if (!ReferenceEquals(ownedHeader, null))
            {
                Object.Destroy(ownedHeader);
            }

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

            if (!TryCreateSeparator(rowTransform))
            {
                return false;
            }

            for (var ingredientIndex = 0;
                ingredientIndex < RecipePresentationModel.MaximumIngredientCount;
                ingredientIndex++)
            {
                if (!TryCreateIngredientCell(rowTransform, rowIndex, ingredientIndex))
                {
                    return false;
                }
            }

            var warningObject = new GameObject("Machine Facility Footer");
            var warningTransform =
                (RectTransform)warningObject.AddComponent(typeof(RectTransform));
            var warningText = (Text)warningObject.AddComponent(typeof(Text));
            if (ReferenceEquals(warningTransform, null) || ReferenceEquals(warningText, null))
            {
                Object.Destroy(warningObject);
                return false;
            }

            warningTransform.SetParent(rowTransform, false);
            ConfigureRect(
                warningTransform,
                RecipeRowLayout.ProductLabelLeft,
                RecipeRowLayout.ProductLabelTop,
                RecipeRowLayout.ProductLabelWidth,
                RecipeRowLayout.ProductLabelHeight);
            ConfigureText(warningText, WarningFontSize, TextAnchor.MiddleLeft);
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
            ConfigureRect(iconTransform, 5f, 2f, RecipeRowLayout.IngredientIconSize, RecipeRowLayout.IngredientIconSize);
            iconImage.raycastTarget = false;

            var textObject = new GameObject("Count");
            var textTransform = (RectTransform)textObject.AddComponent(typeof(RectTransform));
            var countText = (Text)textObject.AddComponent(typeof(Text));
            if (ReferenceEquals(textTransform, null) || ReferenceEquals(countText, null))
            {
                return false;
            }

            textTransform.SetParent(cellTransform, false);
            ConfigureRect(textTransform, 0f, 39f, RecipeRowLayout.IngredientCellWidth, 20f);
            ConfigureText(countText, IngredientFontSize, TextAnchor.MiddleCenter);

            ingredientObjects[rowIndex, ingredientIndex] = cellObject;
            ingredientImages[rowIndex, ingredientIndex] = iconImage;
            ingredientTexts[rowIndex, ingredientIndex] = countText;
            cellObject.SetActive(false);
            return true;
        }

        private bool TryCreateSemanticHeader(RectTransform parent)
        {
            var headerObject = new GameObject("Recipe Semantics");
            var headerTransform =
                (RectTransform)headerObject.AddComponent(typeof(RectTransform));
            if (ReferenceEquals(headerTransform, null))
            {
                Object.Destroy(headerObject);
                return false;
            }

            headerTransform.SetParent(parent, false);
            ConfigureRect(
                headerTransform,
                0f,
                4f,
                PanelGeometry.FixedWidth,
                RecipeRowLayout.HeaderHeight);

            if (!TryCreateHeaderText(headerTransform, "Target Header", "TARGET", 4f, 64f) ||
                !TryCreateHeaderText(
                    headerTransform,
                    "Ingredients Header",
                    "INGREDIENTS",
                    RecipeRowLayout.IngredientFirstLeft,
                    PanelGeometry.FixedWidth - RecipeRowLayout.IngredientFirstLeft - 8f))
            {
                Object.Destroy(headerObject);
                return false;
            }

            semanticHeaderObject = headerObject;
            return true;
        }

        private bool TryCreateHeaderText(
            RectTransform parent,
            string name,
            string copy,
            float left,
            float width)
        {
            var textObject = new GameObject(name);
            var textTransform = (RectTransform)textObject.AddComponent(typeof(RectTransform));
            var text = (Text)textObject.AddComponent(typeof(Text));
            if (ReferenceEquals(textTransform, null) || ReferenceEquals(text, null))
            {
                Object.Destroy(textObject);
                return false;
            }

            textTransform.SetParent(parent, false);
            ConfigureRect(textTransform, left, 0f, width, RecipeRowLayout.HeaderHeight);
            ConfigureText(text, HeaderFontSize, TextAnchor.MiddleCenter);
            text.text = copy;
            text.color = HeaderColor;
            return true;
        }

        private static bool TryCreateSeparator(RectTransform parent)
        {
            var separatorObject = new GameObject("Target Ingredient Separator");
            var separatorTransform =
                (RectTransform)separatorObject.AddComponent(typeof(RectTransform));
            var separatorImage = (Image)separatorObject.AddComponent(typeof(Image));
            if (ReferenceEquals(separatorTransform, null) || ReferenceEquals(separatorImage, null))
            {
                Object.Destroy(separatorObject);
                return false;
            }

            separatorTransform.SetParent(parent, false);
            ConfigureRect(
                separatorTransform,
                RecipeRowLayout.SeparatorLeft,
                2f,
                1f,
                RecipeRowLayout.ContentHeight - 4f);
            separatorImage.color = SeparatorColor;
            separatorImage.raycastTarget = false;
            return true;
        }

        private void ConfigureText(Text text, int fontSize, TextAnchor alignment)
        {
            text.font = nativeFont;
            text.fontSize = fontSize;
            text.alignment = alignment;
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
