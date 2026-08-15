using DSPRecipeTracker;

internal static class RecipeDataSourceTests
{
    public static void Run(Action<bool, string> check)
    {
        CompleteResolution(check);
        MachineCategories(check);
        AvailabilityAndRecovery(check);
        InvalidRemovalAndIsolation(check);
        SuppressionAndRecovery(check);
        InventoryFailureAndRecovery(check);
        FailureContainmentAndRelease(check);
    }

    private static void CompleteResolution(Action<bool, string> check)
    {
        var diagnostics = new RecordingDiagnosticSink();
        var state = new PinnedRecipeState(diagnostics);
        state.Toggle(101);
        state.Toggle(75);

        var recipeAdapter = new RecordingRecipeDataAdapter();
        var universeMatrixItems = new[] { 6001, 6002, 6003, 6004, 6005, 1122 };
        var universeMatrix = CreateData(
            75,
            universeMatrixItems,
            Enumerable.Repeat(1, 6).ToArray(),
            false,
            "Research");
        var handCraftable = CreateData(101, new[] { 1101, 1102 }, new[] { 2, 3 }, true, null);
        recipeAdapter.Set(75, RecipeDataReadResult.Success(universeMatrix));
        recipeAdapter.Set(101, RecipeDataReadResult.Success(handCraftable));

        var inventoryAdapter = new RecordingInventoryDataAdapter();
        foreach (var itemId in universeMatrixItems)
        {
            inventoryAdapter.SetCount(itemId, itemId == 1122 ? 0 : 1);
        }
        inventoryAdapter.SetCount(1101, 2);
        inventoryAdapter.SetCount(1102, 8);

        using var source = new RecipePresentationInputSource(
            state,
            recipeAdapter,
            inventoryAdapter,
            diagnostics);
        var result = source.Collect();

        check(result.Inputs.Select(input => input.RecipeId).SequenceEqual(new[] { 75, 101 }), "recipe data preserves pin order");
        check(result.SuppressedRecipeIds.Count == 0, "complete recipe data suppresses no rows");
        check(result.RemovedRecipeIds.Count == 0, "complete recipe data removes no pins");
        check(result.Inputs[0].IngredientIds.SequenceEqual(universeMatrixItems), "six-input recipe data preserves exact item order");
        check(result.Inputs[0].RequiredCounts.SequenceEqual(Enumerable.Repeat(1, 6)), "six-input recipe data preserves requirements");
        check(result.Inputs[0].CurrentCounts.SequenceEqual(new[] { 1, 1, 1, 1, 1, 0 }), "six-input recipe data reads every Icarus count");
        check(!result.Inputs[0].IsHandCraftable, "machine-only recipe data preserves handcraft state");
        check(result.Inputs[0].ProductionCategory == "Research", "machine-only recipe data preserves native category");
        check(result.Inputs[1].ProductionCategory == null, "hand-craftable recipe data has no category warning");
        check(recipeAdapter.RefreshCalls == 1, "recipe bindings refresh once per collection");
        check(inventoryAdapter.RefreshCalls == 1, "inventory binding refreshes once per collection");
        check(inventoryAdapter.ReadCalls == 8, "inventory adapter reads direct ingredients only");

        var recordsBeforeRepeat = diagnostics.Records.Count;
        source.Collect();
        check(diagnostics.Records.Count == recordsBeforeRepeat, "unchanged adapter availability is diagnostically silent");

        var dataItems = new[] { 41, 42 };
        var dataRequired = new[] { 3, 4 };
        var stableData = CreateData(404, dataItems, dataRequired, true, null);
        dataItems[0] = 999;
        dataRequired[0] = 999;
        check(stableData.IngredientIds[0] == 41, "resolved recipe data owns an ingredient identity snapshot");
        check(stableData.RequiredCounts[0] == 3, "resolved recipe data owns a requirement snapshot");
    }

    private static void MachineCategories(Action<bool, string> check)
    {
        var categories = new[]
        {
            "Smelt",
            "Chemical",
            "Refine",
            "Assemble",
            "Particle",
            "Fractionate",
            "Research"
        };

        for (var index = 0; index < categories.Length; index++)
        {
            var recipeId = 200 + index;
            var itemId = 1200 + index;
            var state = new PinnedRecipeState(new RecordingDiagnosticSink());
            state.Toggle(recipeId);
            var recipeAdapter = new RecordingRecipeDataAdapter();
            recipeAdapter.Set(
                recipeId,
                RecipeDataReadResult.Success(
                    CreateData(recipeId, new[] { itemId }, new[] { 1 }, false, categories[index])));
            var inventoryAdapter = new RecordingInventoryDataAdapter();
            inventoryAdapter.SetCount(itemId, 1);
            using var source = new RecipePresentationInputSource(
                state,
                recipeAdapter,
                inventoryAdapter,
                new RecordingDiagnosticSink());

            var result = source.Collect();
            check(result.Inputs.Single().ProductionCategory == categories[index], categories[index] + " machine category passes through unchanged");
        }
    }

    private static void AvailabilityAndRecovery(Action<bool, string> check)
    {
        var diagnostics = new RecordingDiagnosticSink();
        var state = new PinnedRecipeState(diagnostics);
        state.Toggle(301);
        var recipeAdapter = new RecordingRecipeDataAdapter { Available = false };
        recipeAdapter.Set(301, RecipeDataReadResult.Success(CreateData(301, new[] { 1301 }, new[] { 2 }, true, null)));
        var inventoryAdapter = new RecordingInventoryDataAdapter();
        inventoryAdapter.SetCount(1301, 2);
        using var source = new RecipePresentationInputSource(state, recipeAdapter, inventoryAdapter, diagnostics);

        var unavailable = source.Collect();
        check(unavailable.Inputs.Count == 0, "unavailable recipe adapter suppresses complete row");
        check(unavailable.SuppressedRecipeIds.SequenceEqual(new[] { 301 }), "unavailable recipe adapter identifies suppressed recipe");
        CheckRecipeOrder(check, state.RecipeIds, "unavailable recipe adapter retains pin", 301);

        recipeAdapter.Available = true;
        var recovered = source.Collect();
        check(recovered.Inputs.Single().RecipeId == 301, "recipe adapter recovery restores normalized row");
        check(diagnostics.Records.Count(record => record.Message == "recipe-data adapter=recipe available=false") == 1, "recipe adapter unavailability is diagnosed once");
        check(diagnostics.Records.Count(record => record.Message == "recipe-data adapter=recipe available=true") == 1, "recipe adapter recovery is diagnosed once");

        inventoryAdapter.Available = false;
        var inventoryUnavailable = source.Collect();
        check(inventoryUnavailable.Inputs.Count == 0, "unavailable package suppresses complete row");
        CheckRecipeOrder(check, state.RecipeIds, "unavailable package retains pin", 301);
        inventoryAdapter.Available = true;
        check(source.Collect().Inputs.Single().RecipeId == 301, "package recovery restores normalized row");
        check(diagnostics.Records.Count(record => record.Message == "recipe-data adapter=inventory available=false") == 1, "inventory unavailability is diagnosed once");
        check(diagnostics.Records.Count(record => record.Message == "recipe-data adapter=inventory available=true") == 2, "initial and recovered inventory availability are diagnosed");
    }

    private static void InvalidRemovalAndIsolation(Action<bool, string> check)
    {
        var diagnostics = new RecordingDiagnosticSink();
        var state = new PinnedRecipeState(diagnostics);
        state.Toggle(401);
        state.Toggle(402);
        state.Toggle(403);
        var recipeAdapter = new RecordingRecipeDataAdapter();
        recipeAdapter.Set(403, RecipeDataReadResult.Success(CreateData(403, new[] { 1403 }, new[] { 1 }, true, null)));
        recipeAdapter.Set(402, RecipeDataReadResult.InvalidItem(RecipeDataFailureReason.MissingItemIcon, 1402));
        recipeAdapter.Set(401, RecipeDataReadResult.Success(CreateData(401, new[] { 1401 }, new[] { 1 }, true, null)));
        var inventoryAdapter = new RecordingInventoryDataAdapter();
        inventoryAdapter.SetCount(1403, 1);
        inventoryAdapter.SetCount(1401, 0);
        using var source = new RecipePresentationInputSource(state, recipeAdapter, inventoryAdapter, diagnostics);

        var result = source.Collect();
        check(result.Inputs.Select(input => input.RecipeId).SequenceEqual(new[] { 403, 401 }), "invalid item failure cannot corrupt valid row order");
        check(result.RemovedRecipeIds.SequenceEqual(new[] { 402 }), "missing required item icon requests safe pin removal");
        CheckRecipeOrder(check, state.RecipeIds, "safe invalid-item removal preserves remaining pin order", 403, 401);
        check(diagnostics.Records.Count(record => record.Message == "recipe-data action=remove-invalid recipeId=402 itemId=1402 reason=missing-item-icon") == 1, "invalid item diagnostic identifies bounded recipe and item identities");

        var invalidCases = new[]
        {
            (RecipeId: 411, Result: RecipeDataReadResult.InvalidRecipe(RecipeDataFailureReason.MissingRecipe), Name: "missing recipe"),
            (RecipeId: 412, Result: RecipeDataReadResult.InvalidRecipe(RecipeDataFailureReason.InvalidIngredientShape), Name: "inconsistent input arrays"),
            (RecipeId: 413, Result: RecipeDataReadResult.InvalidItem(RecipeDataFailureReason.MissingItem, 1413), Name: "missing required item")
        };
        foreach (var invalidCase in invalidCases)
        {
            var caseState = new PinnedRecipeState(new RecordingDiagnosticSink());
            caseState.Toggle(invalidCase.RecipeId);
            var caseRecipes = new RecordingRecipeDataAdapter();
            caseRecipes.Set(invalidCase.RecipeId, invalidCase.Result);
            using var caseSource = new RecipePresentationInputSource(
                caseState,
                caseRecipes,
                new RecordingInventoryDataAdapter(),
                new RecordingDiagnosticSink());
            var caseResult = caseSource.Collect();
            check(caseResult.RemovedRecipeIds.SequenceEqual(new[] { invalidCase.RecipeId }), invalidCase.Name + " uses safe removal");
            CheckRecipeOrder(check, caseState.RecipeIds, invalidCase.Name + " leaves no stale pin");
        }
    }

    private static void SuppressionAndRecovery(Action<bool, string> check)
    {
        var diagnostics = new RecordingDiagnosticSink();
        var state = new PinnedRecipeState(diagnostics);
        state.Toggle(501);
        var recipeAdapter = new RecordingRecipeDataAdapter();
        recipeAdapter.Set(
            501,
            RecipeDataReadResult.TemporarilyUnavailable(
                RecipeDataFailureReason.MissingProductionCategory));
        var inventoryAdapter = new RecordingInventoryDataAdapter();
        inventoryAdapter.SetCount(1501, 1);
        using var source = new RecipePresentationInputSource(state, recipeAdapter, inventoryAdapter, diagnostics);

        var suppressed = source.Collect();
        check(suppressed.SuppressedRecipeIds.SequenceEqual(new[] { 501 }), "empty machine category suppresses affected row");
        CheckRecipeOrder(check, state.RecipeIds, "empty machine category retains pin", 501);
        recipeAdapter.Set(501, RecipeDataReadResult.Success(CreateData(501, new[] { 1501 }, new[] { 1 }, false, "Smelt")));
        var recovered = source.Collect();
        check(recovered.Inputs.Single().ProductionCategory == "Smelt", "machine category recovery restores complete row");
        check(diagnostics.Records.Count(record => record.Message == "recipe-data action=suppress recipeId=501 reason=missing-production-category") == 1, "empty category failure is diagnosed once per recipe identity");
    }

    private static void InventoryFailureAndRecovery(Action<bool, string> check)
    {
        var diagnostics = new RecordingDiagnosticSink();
        var state = new PinnedRecipeState(diagnostics);
        state.Toggle(601);
        var recipeAdapter = new RecordingRecipeDataAdapter();
        recipeAdapter.Set(601, RecipeDataReadResult.Success(CreateData(601, new[] { 1601, 1602 }, new[] { 1, 2 }, true, null)));
        var inventoryAdapter = new RecordingInventoryDataAdapter();
        inventoryAdapter.SetCount(1601, 5);
        inventoryAdapter.SetCount(1602, 2);
        inventoryAdapter.FailFor(1602);
        using var source = new RecipePresentationInputSource(state, recipeAdapter, inventoryAdapter, diagnostics);

        var suppressed = source.Collect();
        check(suppressed.Inputs.Count == 0, "partial inventory read never produces partial requirements");
        check(suppressed.SuppressedRecipeIds.SequenceEqual(new[] { 601 }), "inventory read failure suppresses only affected row");
        CheckRecipeOrder(check, state.RecipeIds, "inventory read failure retains pin", 601);
        inventoryAdapter.Recover(1602);
        var recovered = source.Collect();
        check(recovered.Inputs.Single().CurrentCounts.SequenceEqual(new[] { 5, 2 }), "inventory read recovery restores complete counts");
        check(diagnostics.Records.Count(record => record.Message == "recipe-data action=suppress recipeId=601 itemId=1602 reason=inventory-read-failure") == 1, "inventory failure diagnostic identifies bounded item identity");
    }

    private static void FailureContainmentAndRelease(Action<bool, string> check)
    {
        var diagnostics = new RecordingDiagnosticSink();
        var state = new PinnedRecipeState(diagnostics);
        state.Toggle(701);
        var recipeAdapter = new RecordingRecipeDataAdapter
        {
            ThrowOnRead = true,
            ThrowOnRelease = true
        };
        var inventoryAdapter = new RecordingInventoryDataAdapter();
        var source = new RecipePresentationInputSource(state, recipeAdapter, inventoryAdapter, diagnostics);

        var failed = source.Collect();
        check(failed.Inputs.Count == 0 && failed.SuppressedRecipeIds.SequenceEqual(new[] { 701 }), "adapter exception fails softly at row scope");
        CheckRecipeOrder(check, state.RecipeIds, "adapter exception retains pin", 701);
        var failureRecords = diagnostics.Records.Where(record => record.Message.StartsWith("recipe-data action=suppress", StringComparison.Ordinal)).ToList();
        check(failureRecords.Count == 1, "repeated failure identity is bounded");
        source.Collect();
        check(diagnostics.Records.Count(record => record.Message.StartsWith("recipe-data action=suppress", StringComparison.Ordinal)) == 1, "unchanged failure emits no additional diagnostic");
        check(failureRecords.All(record => record.Level == TrackerDiagnosticLevel.Debug), "recipe data failures use Debug level");
        check(failureRecords.All(record => record.Message.Length < 160), "recipe data failures remain concise");
        check(failureRecords.All(record => !record.Message.Contains("object", StringComparison.OrdinalIgnoreCase) && !record.Message.Contains("path", StringComparison.OrdinalIgnoreCase)), "recipe data failures contain no runtime dump or path");

        var recipeRefreshCalls = recipeAdapter.RefreshCalls;
        var inventoryRefreshCalls = inventoryAdapter.RefreshCalls;
        source.Dispose();
        source.Dispose();
        var released = source.Collect();
        check(released.IsReleased && released.Inputs.Count == 0, "released recipe data source is inert");
        check(recipeAdapter.ReleaseCalls == 1, "recipe adapter releases once");
        check(inventoryAdapter.ReleaseCalls == 1, "inventory adapter releases once");
        check(recipeAdapter.RefreshCalls == recipeRefreshCalls, "released source performs no recipe refresh");
        check(inventoryAdapter.RefreshCalls == inventoryRefreshCalls, "released source performs no inventory refresh");
        check(diagnostics.Records.Count(record => record.Message == "recipe-data action=release") == 1, "recipe data release diagnostic is one-time");
    }

    private static ResolvedRecipeData CreateData(
        int recipeId,
        int[] ingredientIds,
        int[] requiredCounts,
        bool isHandCraftable,
        string? productionCategory)
    {
        return new ResolvedRecipeData(
            recipeId,
            new PresentationIconHandle(new object()),
            ingredientIds,
            ingredientIds.Select(_ => new PresentationIconHandle(new object())).ToArray(),
            requiredCounts,
            isHandCraftable,
            productionCategory!);
    }

    private static void CheckRecipeOrder(
        Action<bool, string> check,
        IReadOnlyList<int> actual,
        string name,
        params int[] expected)
    {
        check(actual.SequenceEqual(expected), name);
    }
}

internal sealed class RecordingRecipeDataAdapter : IRecipeDataAdapter
{
    private readonly Dictionary<int, RecipeDataReadResult> results = new Dictionary<int, RecipeDataReadResult>();

    public bool Available { get; set; } = true;

    public bool ThrowOnRefresh { get; set; }

    public bool ThrowOnRead { get; set; }

    public bool ThrowOnRelease { get; set; }

    public int RefreshCalls { get; private set; }

    public int ReadCalls { get; private set; }

    public int ReleaseCalls { get; private set; }

    public void Set(int recipeId, RecipeDataReadResult result)
    {
        results[recipeId] = result;
    }

    public bool TryRefresh()
    {
        RefreshCalls++;
        if (ThrowOnRefresh)
        {
            throw new InvalidOperationException("Recipe bindings unavailable.");
        }

        return Available;
    }

    public RecipeDataReadResult Read(int recipeId)
    {
        ReadCalls++;
        if (ThrowOnRead)
        {
            throw new InvalidOperationException("Recipe read unavailable.");
        }

        return results.TryGetValue(recipeId, out var result)
            ? result
            : RecipeDataReadResult.InvalidRecipe(RecipeDataFailureReason.MissingRecipe);
    }

    public void Release()
    {
        ReleaseCalls++;
        if (ThrowOnRelease)
        {
            throw new InvalidOperationException("Recipe release unavailable.");
        }
    }
}

internal sealed class RecordingInventoryDataAdapter : IInventoryDataAdapter
{
    private readonly Dictionary<int, int> counts = new Dictionary<int, int>();
    private readonly HashSet<int> failedItems = new HashSet<int>();

    public bool Available { get; set; } = true;

    public bool ThrowOnRefresh { get; set; }

    public int RefreshCalls { get; private set; }

    public int ReadCalls { get; private set; }

    public int ReleaseCalls { get; private set; }

    public void SetCount(int itemId, int count)
    {
        counts[itemId] = count;
    }

    public void FailFor(int itemId)
    {
        failedItems.Add(itemId);
    }

    public void Recover(int itemId)
    {
        failedItems.Remove(itemId);
    }

    public bool TryRefresh()
    {
        RefreshCalls++;
        if (ThrowOnRefresh)
        {
            throw new InvalidOperationException("Inventory binding unavailable.");
        }

        return Available;
    }

    public bool TryGetItemCount(int itemId, out int count)
    {
        ReadCalls++;
        if (failedItems.Contains(itemId))
        {
            count = 0;
            return false;
        }

        return counts.TryGetValue(itemId, out count);
    }

    public void Release()
    {
        ReleaseCalls++;
    }
}
