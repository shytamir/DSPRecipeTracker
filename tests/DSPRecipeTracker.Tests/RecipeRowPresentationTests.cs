using DSPRecipeTracker;

internal static class RecipeRowPresentationTests
{
    public static void Run(Action<bool, string> check)
    {
        CompleteThreeRowComposition(check);
        MachineWarningFormatting(check);
        IngredientRangeAndContainment(check);
        UnsupportedAndMalformedRows(check);
        ResourceFailureIsolation(check);
        InitializationAndRelease(check);
    }

    private static void MachineWarningFormatting(Action<bool, string> check)
    {
        var adapter = new RecordingRecipeRowUiAdapter();
        using var presentation = new RecipeRowPresentation(
            adapter,
            new RecordingDiagnosticSink());
        presentation.TryInitialize();

        check(presentation.TryApplyFrame(new RecipePresentationFrame(new[]
        {
            CreateRow(250, new[] { 1250 }, new[] { 1 }, new[] { 1 },
                "  Chemical\u00a0Facility  ")
        })), "machine warning with non-breaking whitespace applies");
        check(adapter.AppliedRows[0].MachineWarning == "Chemical Facility",
            "machine warning whitespace is normalized for the dedicated footer");

        check(RecipeRowPresentation.FormatMachineWarning("Miniature Particle Collider") ==
            "Miniature Particle Collider",
            "multiword machine warning remains complete on one footer line");
    }

    private static void CompleteThreeRowComposition(Action<bool, string> check)
    {
        var diagnostics = new RecordingDiagnosticSink();
        var adapter = new RecordingRecipeRowUiAdapter();
        using var presentation = new RecipeRowPresentation(adapter, diagnostics);
        check(presentation.TryInitialize(), "recipe rows initialize with available resources");

        var first = CreateRow(
            101,
            new[] { 1101, 1102 },
            new[] { 2, 2 },
            new[] { 1, 2 },
            "Smelt");
        var second = CreateRow(202, new[] { 1201 }, new[] { 1 }, new[] { 3 }, null);
        var maximum = CreateRow(
            75,
            new[] { 6001, 6002, 6003, 6004, 6005, 1122 },
            new[] { 1, 1, 1, 1, 1, 1 },
            new[] { 1, 1, 1, 1, 1, 1 },
            "Research");
        var frame = new RecipePresentationFrame(new[] { first, second, maximum });

        check(presentation.TryApplyFrame(frame), "complete three-row frame applies");
        check(adapter.AppliedRows.Count == 3, "three complete rows are applied");
        check(adapter.AppliedRows[0].RecipeId == 101 &&
            adapter.AppliedRows[1].RecipeId == 202 &&
            adapter.AppliedRows[2].RecipeId == 75,
            "complete rows preserve pin order");
        check(adapter.AppliedRows[0].Ingredients[0].ItemId == 1101 &&
            adapter.AppliedRows[0].Ingredients[1].ItemId == 1102,
            "ingredient cells preserve direct-input order");
        check(adapter.AppliedRows[0].Ingredients[0].ComparisonText == "1 / 2" &&
            adapter.AppliedRows[0].Ingredients[1].ComparisonText == "2 / 2",
            "ingredient cells expose current and required text");
        check(adapter.AppliedRows[0].Ingredients[0].Treatment == IngredientValueTreatment.Insufficient &&
            adapter.AppliedRows[0].Ingredients[1].Treatment == IngredientValueTreatment.Sufficient,
            "ingredient cells distinguish insufficient and sufficient values");
        check(adapter.AppliedRows[0].MachineWarning == "Smelt" &&
            adapter.AppliedRows[1].MachineWarning == null,
            "machine-only copy remains beneath only the affected product");
        check(adapter.AppliedRows[2].Ingredients.Select(item => item.ItemId).SequenceEqual(
            new[] { 6001, 6002, 6003, 6004, 6005, 1122 }),
            "recipe 75 exact six-input order is preserved");
        check(diagnostics.Records.Any(record => record.Message ==
            "recipe-rows action=refresh rows=3 applied=3 recipes=[101:2,202:1,75:6] sufficient=8 insufficient=1"),
            "changed row summary is bounded and useful");

        var applyCalls = adapter.ApplyCalls;
        var diagnosticCount = diagnostics.Records.Count;
        check(presentation.TryApplyFrame(frame), "unchanged complete frame remains successful");
        check(adapter.ApplyCalls == applyCalls, "unchanged complete frame does not rebuild rows");
        check(diagnostics.Records.Count == diagnosticCount, "unchanged complete frame is diagnostically silent");
    }

    private static void IngredientRangeAndContainment(Action<bool, string> check)
    {
        for (var count = RecipePresentationModel.MinimumIngredientCount;
            count <= RecipePresentationModel.MaximumIngredientCount;
            count++)
        {
            var ids = Enumerable.Range(1, count).Select(value => 2000 + value).ToArray();
            var required = Enumerable.Repeat(1, count).ToArray();
            var current = Enumerable.Repeat(1, count).ToArray();
            var adapter = new RecordingRecipeRowUiAdapter();
            using var presentation = new RecipeRowPresentation(adapter, new RecordingDiagnosticSink());
            presentation.TryInitialize();
            var applied = presentation.TryApplyFrame(new RecipePresentationFrame(
                new[] { CreateRow(3000 + count, ids, required, current, null) }));
            check(applied && adapter.AppliedRows[0].Ingredients.Count == count,
                "row composition supports " + count + " direct ingredients");
        }

        check(RecipeRowLayout.RowTop(PinnedRecipeState.Capacity - 1) + RecipeRowLayout.RowHeight <=
            PanelGeometry.FixedHeight,
            "third fixed row remains within panel height");
        check(RecipeRowLayout.IngredientLeft(RecipePresentationModel.MaximumIngredientCount - 1) +
            RecipeRowLayout.IngredientCellWidth <= PanelGeometry.FixedWidth,
            "sixth ingredient cell remains within panel width");
        check(RecipeRowLayout.ProductLeft + RecipeRowLayout.ProductSize <
            RecipeRowLayout.IngredientFirstLeft,
            "product and ingredient regions do not overlap");
        check(RecipeRowLayout.HeaderHeight <= RecipeRowLayout.FirstRowTop,
            "semantic headings remain above the first recipe row");
        check(RecipeRowLayout.ProductLabelTop + RecipeRowLayout.ProductLabelHeight <=
            RecipeRowLayout.RowHeight,
            "machine requirement remains inside its recipe row");
        check(RecipeRowLayout.RowHeight == RecipeRowLayout.RowSpacing,
            "machine requirement consumes only the established inter-row reserve");
        check(RecipeRowLayout.ProductLabelTop >= RecipeRowLayout.ContentHeight &&
            RecipeRowLayout.ProductLabelLeft == RecipeRowLayout.ProductLeft &&
            RecipeRowLayout.ProductLabelLeft + RecipeRowLayout.ProductLabelWidth <=
                PanelGeometry.FixedWidth,
            "machine requirement has a dedicated full-width footer below row content");
        check(RecipeRowLayout.ProductLeft + RecipeRowLayout.ProductSize <
            RecipeRowLayout.SeparatorLeft &&
            RecipeRowLayout.SeparatorLeft < RecipeRowLayout.IngredientFirstLeft,
            "separator remains between target and ingredient regions");
    }

    private static void UnsupportedAndMalformedRows(Action<bool, string> check)
    {
        var diagnostics = new RecordingDiagnosticSink();
        var adapter = new RecordingRecipeRowUiAdapter();
        using var presentation = new RecipeRowPresentation(adapter, diagnostics);
        presentation.TryInitialize();

        var sevenIngredients = Enumerable.Range(1, 7)
            .Select(value => new IngredientPresentation(
                value,
                new PresentationIconHandle(new object()),
                1,
                1))
            .ToArray();
        var unsupported = new RecipePresentationRow(
            401,
            new PresentationIconHandle(new object()),
            sevenIngredients,
            null);
        check(!presentation.TryApplyFrame(new RecipePresentationFrame(new[] { unsupported })),
            "unsupported seven-input row is suppressed");
        check(adapter.AppliedRows.Count == 0 && adapter.HiddenRows.Contains(0),
            "unsupported row never renders partially");

        var malformed = new RecipePresentationRow(
            402,
            new PresentationIconHandle(new object()),
            new[]
            {
                new IngredientPresentation(
                    1402,
                    new PresentationIconHandle(new object()),
                    0,
                    1)
            },
            null);
        check(!presentation.TryApplyFrame(new RecipePresentationFrame(new[] { malformed })),
            "malformed count row is suppressed");
        check(diagnostics.Records.Count(record => record.Message ==
            "recipe-rows action=suppress recipeId=401 resource=row-container") == 1,
            "unsupported row failure is bounded by recipe and resource");
        check(diagnostics.Records.Count(record => record.Message ==
            "recipe-rows action=suppress recipeId=402 resource=text") == 1,
            "malformed text failure is bounded by recipe and resource");
    }

    private static void ResourceFailureIsolation(Action<bool, string> check)
    {
        var diagnostics = new RecordingDiagnosticSink();
        var adapter = new RecordingRecipeRowUiAdapter();
        adapter.FailRecipe(502, RecipeRowUiResourceClass.IngredientIcon);
        using var presentation = new RecipeRowPresentation(adapter, diagnostics);
        presentation.TryInitialize();
        var frame = new RecipePresentationFrame(new[]
        {
            CreateRow(501, new[] { 1501 }, new[] { 1 }, new[] { 1 }, null),
            CreateRow(502, new[] { 1502 }, new[] { 1 }, new[] { 0 }, null),
            CreateRow(503, new[] { 1503 }, new[] { 1 }, new[] { 1 }, "Assemble")
        });

        check(!presentation.TryApplyFrame(frame), "one missing row resource reports partial failure");
        check(adapter.AppliedRows.ContainsKey(0) &&
            !adapter.AppliedRows.ContainsKey(1) &&
            adapter.AppliedRows.ContainsKey(2),
            "one missing row resource does not disable neighboring rows");
        check(adapter.HiddenRows.Contains(1), "failed row is hidden completely");
        check(diagnostics.Records.Count(record => record.Message ==
            "recipe-rows action=suppress recipeId=502 resource=ingredient-icon") == 1,
            "resource failure identifies bounded recipe and resource class");

        var diagnosticCount = diagnostics.Records.Count;
        check(!presentation.TryApplyFrame(frame), "unchanged failed frame retries for recovery");
        check(diagnostics.Records.Count == diagnosticCount,
            "unchanged resource failure retry is diagnostically silent");

        adapter.ClearFailure(502);
        check(presentation.TryApplyFrame(frame), "row resource recovery applies the complete frame");
        check(adapter.AppliedRows.ContainsKey(1), "recovered row returns without disturbing order");

        var replacement = new RecipePresentationFrame(new[]
        {
            CreateRow(504, new[] { 1504 }, new[] { 1 }, new[] { 1 }, null)
        });
        adapter.FailRecipe(504, RecipeRowUiResourceClass.ProductIcon);
        check(!presentation.TryApplyFrame(replacement),
            "failed replacement frame invalidates the successful-frame cache");
        var callsBeforeRestore = adapter.ApplyCalls;
        check(presentation.TryApplyFrame(frame),
            "previous successful frame reapplies after an intervening failure");
        check(adapter.ApplyCalls == callsBeforeRestore + frame.Rows.Count,
            "intervening failure cannot leave stale rows behind a cache hit");
    }

    private static void InitializationAndRelease(Action<bool, string> check)
    {
        var unavailableDiagnostics = new RecordingDiagnosticSink();
        var unavailableAdapter = new RecordingRecipeRowUiAdapter
        {
            InitializeResult = false,
            InitializeFailure = new RecipeRowUiFailure(0, RecipeRowUiResourceClass.NativeFont)
        };
        using (var unavailable = new RecipeRowPresentation(unavailableAdapter, unavailableDiagnostics))
        {
            check(!unavailable.TryInitialize(), "missing native font disables row presentation softly");
            check(unavailableAdapter.ReleaseCalls == 1,
                "partial row initialization releases owned resources once");
        }
        check(unavailableAdapter.ReleaseCalls == 1,
            "disposing disabled row presentation does not release twice");
        check(unavailableDiagnostics.Records.Any(record => record.Message ==
            "recipe-rows action=suppress recipeId=0 resource=native-font"),
            "missing native font emits one bounded diagnostic");

        var diagnostics = new RecordingDiagnosticSink();
        var adapter = new RecordingRecipeRowUiAdapter();
        var presentation = new RecipeRowPresentation(adapter, diagnostics);
        presentation.TryInitialize();
        presentation.Dispose();
        presentation.Dispose();
        check(adapter.ReleaseCalls == 1, "row resources release once");
        check(!presentation.TryApplyFrame(new RecipePresentationFrame(Array.Empty<RecipePresentationRow>())),
            "released row presentation is inert");
        check(diagnostics.Records.Count(record => record.Message == "recipe-rows action=release") == 1,
            "row release diagnostic is one-time");
    }

    private static RecipePresentationRow CreateRow(
        int recipeId,
        int[] itemIds,
        int[] required,
        int[] current,
        string? machineWarning)
    {
        var ingredients = new IngredientPresentation[itemIds.Length];
        for (var index = 0; index < itemIds.Length; index++)
        {
            ingredients[index] = new IngredientPresentation(
                itemIds[index],
                new PresentationIconHandle(new object()),
                required[index],
                current[index]);
        }

        return new RecipePresentationRow(
            recipeId,
            new PresentationIconHandle(new object()),
            ingredients,
            machineWarning);
    }
}

internal sealed class RecordingRecipeRowUiAdapter : IRecipeRowUiAdapter
{
    private readonly Dictionary<int, RecipeRowUiFailure> failures = new();

    public bool InitializeResult { get; set; } = true;

    public RecipeRowUiFailure InitializeFailure { get; set; }

    public int ApplyCalls { get; private set; }

    public int ReleaseCalls { get; private set; }

    public Dictionary<int, RecipeRowView> AppliedRows { get; } = new();

    public HashSet<int> HiddenRows { get; } = new();

    public void FailRecipe(int recipeId, RecipeRowUiResourceClass resourceClass)
    {
        failures[recipeId] = new RecipeRowUiFailure(recipeId, resourceClass);
    }

    public void ClearFailure(int recipeId)
    {
        failures.Remove(recipeId);
    }

    public bool TryInitialize(out RecipeRowUiFailure failure)
    {
        failure = InitializeFailure;
        return InitializeResult;
    }

    public bool TryApplyRow(
        int rowIndex,
        RecipeRowView row,
        out RecipeRowUiFailure failure)
    {
        ApplyCalls++;
        if (failures.TryGetValue(row.RecipeId, out failure))
        {
            AppliedRows.Remove(rowIndex);
            return false;
        }

        failure = default;
        AppliedRows[rowIndex] = row;
        HiddenRows.Remove(rowIndex);
        return true;
    }

    public bool TryHideRow(int rowIndex)
    {
        AppliedRows.Remove(rowIndex);
        HiddenRows.Add(rowIndex);
        return true;
    }

    public void Release()
    {
        ReleaseCalls++;
        AppliedRows.Clear();
    }
}
