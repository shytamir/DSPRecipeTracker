using DSPRecipeTracker;

internal static class LiveRecipePresentationTests
{
    public static void Run(Action<bool, string> check)
    {
        ChangedAndUnchangedRefresh(check);
        IngredientRange(check);
        SuppressionRemovalAndRecovery(check);
        RowFailureAndEmptyState(check);
        InitializationAndRelease(check);
    }

    private static void ChangedAndUnchangedRefresh(Action<bool, string> check)
    {
        using var fixture = new LivePresentationFixture();
        fixture.PinInOrder(101);
        fixture.SetRecipe(101, new[] { 1101 }, new[] { 2 });
        fixture.Inventory.SetCount(1101, 1);

        check(fixture.Live.TryInitialize(), "live presentation initializes");
        fixture.Live.Refresh();
        check(fixture.Rows.AppliedRows[0].Ingredients[0].ComparisonText == "1 / 2",
            "initial live row reads Icarus inventory");
        check(fixture.Rows.AppliedRows[0].Ingredients[0].Treatment ==
            IngredientValueTreatment.Insufficient,
            "initial live row is insufficient below the exact threshold");

        var recipeRefreshCalls = fixture.Recipes.RefreshCalls;
        var inventoryRefreshCalls = fixture.Inventory.RefreshCalls;
        var rowApplyCalls = fixture.Rows.ApplyCalls;
        for (var call = 0;
            call < LiveRecipePresentation.SteadyRefreshCallInterval - 1;
            call++)
        {
            fixture.Live.Refresh();
        }

        check(fixture.Recipes.RefreshCalls == recipeRefreshCalls &&
            fixture.Inventory.RefreshCalls == inventoryRefreshCalls,
            "steady live refresh fast path performs no data collection");
        check(fixture.Rows.ApplyCalls == rowApplyCalls,
            "steady live refresh fast path performs no Unity application");

        fixture.Inventory.SetCount(1101, 2);
        fixture.Live.Refresh();
        check(fixture.Rows.AppliedRows[0].Ingredients[0].ComparisonText == "2 / 2" &&
            fixture.Rows.AppliedRows[0].Ingredients[0].Treatment ==
                IngredientValueTreatment.Sufficient,
            "scheduled inventory refresh crosses the exact sufficiency threshold");
        check(fixture.State.RecipeIds.SequenceEqual(new[] { 101 }),
            "inventory refresh and crafting-equivalent count changes preserve pin order");

        var diagnosticCount = fixture.Diagnostics.Records.Count;
        rowApplyCalls = fixture.Rows.ApplyCalls;
        fixture.AdvanceToScheduledRefresh();
        check(fixture.Rows.ApplyCalls == rowApplyCalls,
            "unchanged normalized frame does not reapply Unity rows");
        check(fixture.Diagnostics.Records.Count == diagnosticCount,
            "unchanged scheduled refresh is diagnostically silent");

        fixture.SetRecipe(202, new[] { 1201 }, new[] { 1 });
        fixture.Inventory.SetCount(1201, 1);
        fixture.State.Toggle(202);
        recipeRefreshCalls = fixture.Recipes.RefreshCalls;
        fixture.Live.Refresh();
        check(fixture.Recipes.RefreshCalls == recipeRefreshCalls + 1,
            "pin change bypasses the steady refresh interval");
        check(fixture.Rows.AppliedRows[0].RecipeId == 202 &&
            fixture.Rows.AppliedRows[1].RecipeId == 101,
            "pin-change refresh preserves newest-first order");
    }

    private static void IngredientRange(Action<bool, string> check)
    {
        for (var count = RecipePresentationModel.MinimumIngredientCount;
            count <= RecipePresentationModel.MaximumIngredientCount;
            count++)
        {
            using var fixture = new LivePresentationFixture();
            var recipeId = 300 + count;
            var itemIds = Enumerable.Range(1, count)
                .Select(value => 3000 + value)
                .ToArray();
            fixture.PinInOrder(recipeId);
            fixture.SetRecipe(recipeId, itemIds, Enumerable.Repeat(1, count).ToArray());
            foreach (var itemId in itemIds)
            {
                fixture.Inventory.SetCount(itemId, 1);
            }

            fixture.Live.TryInitialize();
            fixture.Live.Refresh();
            check(fixture.Rows.AppliedRows[0].Ingredients.Count == count,
                "live refresh supports " + count + " direct ingredients");
        }
    }

    private static void SuppressionRemovalAndRecovery(Action<bool, string> check)
    {
        using var fixture = new LivePresentationFixture();
        fixture.PinInOrder(401, 402);
        fixture.SetRecipe(401, new[] { 1401 }, new[] { 1 });
        fixture.SetRecipe(402, new[] { 1402 }, new[] { 1 });
        fixture.Inventory.SetCount(1401, 1);
        fixture.Inventory.SetCount(1402, 1);
        fixture.Inventory.Available = false;

        fixture.Live.TryInitialize();
        fixture.Live.Refresh();
        check(fixture.State.RecipeIds.SequenceEqual(new[] { 401, 402 }),
            "unavailable inventory retains all pins");
        check(fixture.Rows.AppliedRows.Count == 0,
            "unavailable inventory suppresses complete rows");

        fixture.Inventory.Available = true;
        fixture.AdvanceToScheduledRefresh();
        check(fixture.Rows.AppliedRows[0].RecipeId == 401 &&
            fixture.Rows.AppliedRows[1].RecipeId == 402,
            "inventory recovery restores rows in pin order");

        fixture.Inventory.FailFor(1401);
        fixture.AdvanceToScheduledRefresh();
        check(fixture.State.RecipeIds.SequenceEqual(new[] { 401, 402 }),
            "temporary row inventory failure retains pin order");
        check(fixture.Rows.AppliedRows.Count == 1 &&
            fixture.Rows.AppliedRows[0].RecipeId == 402,
            "temporary row failure suppresses only the affected row");

        fixture.Inventory.Recover(1401);
        fixture.AdvanceToScheduledRefresh();
        check(fixture.Rows.AppliedRows[0].RecipeId == 401 &&
            fixture.Rows.AppliedRows[1].RecipeId == 402,
            "row recovery restores the affected row without re-pinning");

        fixture.Recipes.Set(
            401,
            RecipeDataReadResult.InvalidRecipe(RecipeDataFailureReason.MissingRecipe));
        fixture.AdvanceToScheduledRefresh();
        check(fixture.State.RecipeIds.SequenceEqual(new[] { 402 }),
            "invalid recipe evidence uses the accepted safe-removal path");
        check(fixture.Rows.AppliedRows.Count == 1 &&
            fixture.Rows.AppliedRows[0].RecipeId == 402,
            "invalid removal preserves the remaining relative order");

        var refreshRecords = fixture.Diagnostics.Records
            .Where(record => record.Message.StartsWith(
                "live-recipe-refresh action=refresh",
                StringComparison.Ordinal))
            .ToList();
        check(refreshRecords.Any(record => record.Message.Contains("suppressed=[401]")) &&
            refreshRecords.Any(record => record.Message.Contains("suppressed=[]")),
            "changed diagnostics identify row suppression and recovery");
        check(refreshRecords.All(record => record.Message.Length < 180),
            "live refresh diagnostics remain bounded");
    }

    private static void RowFailureAndEmptyState(Action<bool, string> check)
    {
        using var fixture = new LivePresentationFixture();
        fixture.PinInOrder(501);
        fixture.SetRecipe(501, new[] { 1501 }, new[] { 1 });
        fixture.Inventory.SetCount(1501, 1);
        fixture.Rows.FailRecipe(501, RecipeRowUiResourceClass.ProductIcon);

        fixture.Live.TryInitialize();
        fixture.Live.Refresh();
        check(fixture.State.RecipeIds.SequenceEqual(new[] { 501 }),
            "row resource failure retains the pin");
        check(fixture.Rows.AppliedRows.Count == 0,
            "row resource failure suppresses the affected row");

        fixture.Rows.ClearFailure(501);
        fixture.AdvanceToScheduledRefresh();
        check(fixture.Rows.AppliedRows[0].RecipeId == 501,
            "unchanged-frame row retry recovers at the bounded cadence");

        fixture.State.Toggle(501);
        var recipeRefreshCalls = fixture.Recipes.RefreshCalls;
        fixture.Live.Refresh();
        check(fixture.Rows.AppliedRows.Count == 0,
            "empty pin transition hides all live rows immediately");
        check(fixture.Recipes.RefreshCalls == recipeRefreshCalls,
            "empty state performs no recipe or inventory collection");
        var emptyDiagnosticCount = fixture.Diagnostics.Records.Count;
        fixture.AdvanceToScheduledRefresh();
        fixture.AdvanceToScheduledRefresh();
        check(fixture.Recipes.RefreshCalls == recipeRefreshCalls,
            "steady empty state remains free of data collection");
        check(fixture.Diagnostics.Records.Count == emptyDiagnosticCount,
            "steady empty state remains diagnostically silent");
    }

    private static void InitializationAndRelease(Action<bool, string> check)
    {
        var unavailable = new LivePresentationFixture();
        unavailable.Rows.InitializeResult = false;
        unavailable.Rows.InitializeFailure = new RecipeRowUiFailure(
            0,
            RecipeRowUiResourceClass.NativeFont);
        check(!unavailable.Live.TryInitialize(),
            "missing row resources disable live presentation softly");
        unavailable.Live.Refresh();
        check(unavailable.Recipes.RefreshCalls == 0 && unavailable.Inventory.RefreshCalls == 0,
            "disabled live presentation is inert");
        unavailable.Dispose();
        check(unavailable.Rows.ReleaseCalls == 1,
            "failed initialization and disposal release row resources once");

        var fixture = new LivePresentationFixture();
        fixture.PinInOrder(601);
        fixture.SetRecipe(601, new[] { 1601 }, new[] { 1 });
        fixture.Inventory.SetCount(1601, 1);
        fixture.Live.TryInitialize();
        fixture.Live.Refresh();
        fixture.Dispose();
        fixture.Dispose();
        var recipeRefreshCalls = fixture.Recipes.RefreshCalls;
        fixture.Live.Refresh();
        check(fixture.Recipes.ReleaseCalls == 1 && fixture.Inventory.ReleaseCalls == 1,
            "live data adapters release once");
        check(fixture.Rows.ReleaseCalls == 1,
            "live row presentation releases once");
        check(fixture.Recipes.RefreshCalls == recipeRefreshCalls,
            "released live presentation is inert");
        check(fixture.Diagnostics.Records.Count(record => record.Message ==
            "live-recipe-refresh action=release") == 1,
            "live presentation release diagnostic is one-time");
    }

    private sealed class LivePresentationFixture : IDisposable
    {
        public LivePresentationFixture()
        {
            State = new PinnedRecipeState(Diagnostics);
            Source = new RecipePresentationInputSource(
                State,
                Recipes,
                Inventory,
                Diagnostics);
            RowPresentation = new RecipeRowPresentation(Rows, Diagnostics);
            Live = new LiveRecipePresentation(
                State,
                Source,
                new RecipePresentationModel(Diagnostics),
                RowPresentation,
                Diagnostics);
        }

        public RecordingDiagnosticSink Diagnostics { get; } = new();

        public PinnedRecipeState State { get; }

        public RecordingRecipeDataAdapter Recipes { get; } = new();

        public RecordingInventoryDataAdapter Inventory { get; } = new();

        public RecordingRecipeRowUiAdapter Rows { get; } = new();

        public RecipePresentationInputSource Source { get; }

        public RecipeRowPresentation RowPresentation { get; }

        public LiveRecipePresentation Live { get; }

        public void PinInOrder(params int[] recipeIds)
        {
            for (var index = recipeIds.Length - 1; index >= 0; index--)
            {
                State.Toggle(recipeIds[index]);
            }
        }

        public void SetRecipe(int recipeId, int[] ingredientIds, int[] requiredCounts)
        {
            Recipes.Set(
                recipeId,
                RecipeDataReadResult.Success(new ResolvedRecipeData(
                    recipeId,
                    new PresentationIconHandle(new object()),
                    ingredientIds,
                    ingredientIds
                        .Select(_ => new PresentationIconHandle(new object()))
                        .ToArray(),
                    requiredCounts,
                    true,
                    null!)));
        }

        public void AdvanceToScheduledRefresh()
        {
            for (var call = 0;
                call < LiveRecipePresentation.SteadyRefreshCallInterval;
                call++)
            {
                Live.Refresh();
            }
        }

        public void Dispose()
        {
            Live.Dispose();
        }
    }
}
