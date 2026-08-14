using DSPRecipeTracker;

var failures = new List<string>();

Check(PluginMetadata.Guid == "dsprecipetracker", "plugin GUID");
Check(PluginMetadata.DisplayName == "DSP-Recipe-Tracker", "plugin display name");
Check(BuildIdentity.SemanticVersion == $"{BuildIdentity.Major}.{BuildIdentity.Minor}.{BuildIdentity.Build}", "semantic version");
Check(BuildIdentity.AssemblyVersion == BuildIdentity.SemanticVersion + ".0", "assembly version");
Check(BuildIdentity.DiagnosticLabel.StartsWith(BuildIdentity.SemanticVersion + ".", StringComparison.Ordinal), "diagnostic label");
Check(BuildIdentity.DiagnosticLabel != BuildIdentity.SemanticVersion, "diagnostic label is not loader identity");

var stateDiagnostics = new RecordingDiagnosticSink();
var pinnedRecipes = new PinnedRecipeState(stateDiagnostics);
CheckRecipeOrder(pinnedRecipes.RecipeIds, "initial pin order");

var pinTen = pinnedRecipes.Toggle(10);
Check(pinTen.Kind == PinStateChangeKind.Pinned, "first pin transition");
Check(pinTen.RecipeId == 10, "first pin recipe identity");
Check(pinTen.EvictedRecipeId == null, "first pin has no eviction");
CheckRecipeOrder(pinnedRecipes.RecipeIds, "first pin order", 10);

pinnedRecipes.Toggle(20);
pinnedRecipes.Toggle(30);
CheckRecipeOrder(pinnedRecipes.RecipeIds, "three-pin order", 30, 20, 10);

var pinForty = pinnedRecipes.Toggle(40);
Check(pinForty.Kind == PinStateChangeKind.Pinned, "fourth pin transition");
Check(pinForty.EvictedRecipeId == 10, "fourth pin evicts bottom entry");
CheckRecipeOrder(pinnedRecipes.RecipeIds, "fourth pin FILO order", 40, 30, 20);

var unpinMiddle = pinnedRecipes.Toggle(30);
Check(unpinMiddle.Kind == PinStateChangeKind.Unpinned, "explicit unpin transition");
CheckRecipeOrder(pinnedRecipes.RecipeIds, "middle unpin preserves order", 40, 20);

pinnedRecipes.Toggle(30);
CheckRecipeOrder(pinnedRecipes.RecipeIds, "repin inserts at top without duplicate", 30, 40, 20);

var removeMiddle = pinnedRecipes.RemoveUnavailable(40);
Check(removeMiddle.Kind == PinStateChangeKind.UnavailableRemoved, "unavailable removal transition");
CheckRecipeOrder(pinnedRecipes.RecipeIds, "unavailable removal preserves order", 30, 20);

var diagnosticCountBeforeNoOp = stateDiagnostics.Records.Count;
var removeMissing = pinnedRecipes.RemoveUnavailable(999);
Check(!removeMissing.Changed, "missing unavailable recipe is a no-op");
CheckRecipeOrder(pinnedRecipes.RecipeIds, "no-op removal preserves order", 30, 20);
Check(stateDiagnostics.Records.Count == diagnosticCountBeforeNoOp, "no-op transition is diagnostically silent");

Check(stateDiagnostics.Records.Count == 7, "one diagnostic per completed pin transition");
Check(stateDiagnostics.Records.All(record => record.Level == TrackerDiagnosticLevel.Debug), "pin diagnostics use Debug level");
Check(stateDiagnostics.Records.All(record => record.Message.StartsWith("tracker-state action=", StringComparison.Ordinal)), "pin diagnostics identify action");
Check(stateDiagnostics.Records.All(record => record.Message.Contains(" recipeId=", StringComparison.Ordinal)), "pin diagnostics identify affected recipe");
Check(stateDiagnostics.Records.All(record => record.Message.Contains(" order=[", StringComparison.Ordinal)), "pin diagnostics identify resulting order");
Check(stateDiagnostics.Records.All(record => record.Message.Length < 128), "pin diagnostics remain bounded");
Check(stateDiagnostics.Records[3].Message.Contains("evictedRecipeId=10", StringComparison.Ordinal), "eviction diagnostic identifies removed recipe");

var inputDiagnostics = new RecordingDiagnosticSink();
var inputState = new PinnedRecipeState(inputDiagnostics);
var inputAdapter = new RecordingReplicatorPinInputAdapter();
using (var input = new ReplicatorPinInput(inputAdapter, inputState, inputDiagnostics))
{
    Check(input.TryInitialize(), "Replicator input initializes");
    Check(input.IsAvailable, "Replicator input reports availability");
    Check(inputAdapter.AttachCalls == 1, "Replicator input listener attaches once");

    inputAdapter.RecipeIds = new int?[] { 101 };
    inputAdapter.Raise(ReplicatorPointerButton.Left);
    inputAdapter.Raise(ReplicatorPointerButton.Middle);
    CheckRecipeOrder(inputState.RecipeIds, "left and middle clicks do not enter tracker state");

    inputAdapter.Raise(ReplicatorPointerButton.Right);
    Check(inputAdapter.NativeSelectionCalls == 3, "native selection runs for every pointer-down event");
    Check(inputAdapter.NativeSelectionObservedBeforeTracker, "native selection runs before tracker listener");
    CheckRecipeOrder(inputState.RecipeIds, "right click pins populated recipe", 101);

    inputAdapter.Raise(ReplicatorPointerButton.Right);
    CheckRecipeOrder(inputState.RecipeIds, "second right click unpins populated recipe");
}
Check(inputAdapter.ReleaseCalls == 1, "Replicator input listener releases once");
Check(inputAdapter.ListenerCount == 0, "Replicator input removes only its listener");
inputAdapter.Raise(ReplicatorPointerButton.Right);
CheckRecipeOrder(inputState.RecipeIds, "released Replicator input remains inert");
Check(inputDiagnostics.Records.Count(record => record.Message.StartsWith("replicator-pin-input action=", StringComparison.Ordinal)) == 4, "Replicator input diagnostics cover attach, accepted gestures, and detach only");
Check(inputDiagnostics.Records.Any(record => record.Message == "replicator-pin-input action=pin gridIndex=0 recipeId=101"), "accepted pin diagnostic identifies index, recipe, and action");
Check(inputDiagnostics.Records.Any(record => record.Message == "replicator-pin-input action=unpin gridIndex=0 recipeId=101"), "accepted unpin diagnostic identifies index, recipe, and action");
Check(inputDiagnostics.Records.Any(record => record.Message == "replicator-pin-input action=detach"), "listener removal diagnostic is emitted once");
Check(inputDiagnostics.Records.Where(record => record.Message.StartsWith("replicator-pin-input action=", StringComparison.Ordinal)).All(record => record.Level == TrackerDiagnosticLevel.Debug), "Replicator input diagnostics use Debug level");
Check(inputDiagnostics.Records.Where(record => record.Message.StartsWith("replicator-pin-input action=", StringComparison.Ordinal)).All(record => record.Message.Length < 128), "Replicator input diagnostics remain bounded");

var invalidDiagnostics = new RecordingDiagnosticSink();
var invalidState = new PinnedRecipeState(invalidDiagnostics);
var invalidAdapter = new RecordingReplicatorPinInputAdapter
{
    CurrentRecipeIndex = -1,
    RecipeIds = new int?[] { 201 }
};
using (var input = new ReplicatorPinInput(invalidAdapter, invalidState, invalidDiagnostics))
{
    Check(input.TryInitialize(), "invalid-recipe input initializes");
    invalidAdapter.Raise(ReplicatorPointerButton.Right);
    invalidAdapter.Raise(ReplicatorPointerButton.Right);
    Check(!input.IsAvailable, "invalid populated recipe fails softly");
}
CheckRecipeOrder(invalidState.RecipeIds, "invalid recipe never reaches tracker state");
Check(invalidDiagnostics.Records.Count(record => record.Message.Contains("action=disable", StringComparison.Ordinal)) == 1, "invalid recipe failure is reported once");
Check(invalidAdapter.ReleaseCalls == 1, "invalid recipe listener cleanup is one-time");

foreach (var invalidIndexAdapter in new[]
{
    new RecordingReplicatorPinInputAdapter { CurrentRecipeIndex = 1, RecipeIds = new int?[] { 201 } },
    new RecordingReplicatorPinInputAdapter { CurrentRecipeIndex = 0, RecipeIds = new int?[] { null } }
})
{
    var diagnostics = new RecordingDiagnosticSink();
    var state = new PinnedRecipeState(diagnostics);
    using var input = new ReplicatorPinInput(invalidIndexAdapter, state, diagnostics);
    Check(input.TryInitialize(), "invalid index case initializes");
    invalidIndexAdapter.Raise(ReplicatorPointerButton.Right);
    CheckRecipeOrder(state.RecipeIds, "out-of-range or unpopulated recipe is rejected");
}

var failedAttachDiagnostics = new RecordingDiagnosticSink();
var failedAttachAdapter = new RecordingReplicatorPinInputAdapter { AttachResult = false };
using (var input = new ReplicatorPinInput(failedAttachAdapter, new PinnedRecipeState(failedAttachDiagnostics), failedAttachDiagnostics))
{
    Check(!input.TryInitialize(), "failed input binding fails softly");
    Check(!input.TryInitialize(), "failed input binding cannot attach again");
}
Check(failedAttachAdapter.ReleaseCalls == 1, "failed input binding cleanup is one-time");
Check(failedAttachDiagnostics.Records.Count(record => record.Message.Contains("action=disable", StringComparison.Ordinal)) == 1, "failed input binding diagnostic is emitted once");

var treatmentDiagnostics = new RecordingDiagnosticSink();
var treatmentAdapter = new RecordingRecipeGridTreatmentAdapter();
treatmentAdapter.SetPopulation(10, 20, 30);
using (var treatment = new RecipeGridTreatment(treatmentAdapter, treatmentDiagnostics))
{
    Check(treatment.TryInitialize(), "recipe-grid treatment initializes");
    Check(treatment.IsAvailable, "recipe-grid treatment reports availability");
    Check(treatmentAdapter.InitializeCalls == 1, "recipe-grid treatment resources initialize once");
    Check(treatment.TryRefresh(new[] { 20 }), "initial recipe-grid treatment refresh succeeds");
    Check(treatmentAdapter.ApplyCalls == 1, "initial recipe-grid treatment uploads once");
    CheckTreatmentState(treatmentAdapter.LastAppliedState, "initial recipe-grid treatment", 0, RecipeGridTreatmentModel.UnpinnedMask);
    CheckTreatmentState(treatmentAdapter.LastAppliedState, "pinned recipe-grid treatment", 1, RecipeGridTreatmentModel.PinnedMask);
    CheckTreatmentState(treatmentAdapter.LastAppliedState, "second unpinned recipe-grid treatment", 2, RecipeGridTreatmentModel.UnpinnedMask);
    CheckTreatmentState(treatmentAdapter.LastAppliedState, "initial neutral recipe-grid treatment", 3, RecipeGridTreatmentModel.NeutralMask);
    Check(treatmentAdapter.OriginalState.All(value => value == 99), "tracker never writes the simulated native state buffer");

    Check(treatment.TryRefresh(new[] { 20 }), "unchanged recipe-grid treatment refresh succeeds");
    Check(treatmentAdapter.ApplyCalls == 1, "unchanged recipe-grid treatment suppresses upload");

    Check(treatment.TryRefresh(new[] { 30 }), "pin-change recipe-grid treatment refresh succeeds");
    Check(treatmentAdapter.ApplyCalls == 2, "pin change uploads once");
    CheckTreatmentState(treatmentAdapter.LastAppliedState, "unpin remaps to green", 1, RecipeGridTreatmentModel.UnpinnedMask);
    CheckTreatmentState(treatmentAdapter.LastAppliedState, "new pin remaps to red", 2, RecipeGridTreatmentModel.PinnedMask);

    treatmentAdapter.SetPopulation(40);
    Check(treatment.TryRefresh(new[] { 30 }), "repopulated recipe-grid treatment refresh succeeds");
    Check(treatmentAdapter.ApplyCalls == 3, "population change uploads once");
    CheckTreatmentState(treatmentAdapter.LastAppliedState, "repopulated cell is unpinned", 0, RecipeGridTreatmentModel.UnpinnedMask);
    CheckTreatmentState(treatmentAdapter.LastAppliedState, "stale cell one is cleared", 1, RecipeGridTreatmentModel.NeutralMask);
    CheckTreatmentState(treatmentAdapter.LastAppliedState, "stale cell two is cleared", 2, RecipeGridTreatmentModel.NeutralMask);
    Check(treatmentAdapter.LastAppliedState.Count(value => value != RecipeGridTreatmentModel.NeutralMask) == 1, "pins absent from current grid remain neutral");

    treatmentAdapter.SetPopulation(50);
    Check(treatment.TryRefresh(new[] { 30 }), "same-mask population identity change refresh succeeds");
    Check(treatmentAdapter.ApplyCalls == 4, "population identity change uploads even when visible mask is unchanged");
}
Check(treatmentAdapter.ReleaseCalls == 1, "recipe-grid treatment resources release once");
Check(treatmentDiagnostics.Records.Count(record => record.Message.Contains("action=refresh", StringComparison.Ordinal)) == 4, "changed recipe-grid treatment refreshes emit diagnostics only");
Check(treatmentDiagnostics.Records.Any(record => record.Message == "recipe-grid-treatment action=refresh populated=3 unpinned=2 pinned=1"), "recipe-grid treatment diagnostic reports changed counts");
Check(treatmentDiagnostics.Records.Any(record => record.Message == "recipe-grid-treatment action=release"), "recipe-grid treatment release diagnostic is emitted");
Check(treatmentDiagnostics.Records.Where(record => record.Message.StartsWith("recipe-grid-treatment action=", StringComparison.Ordinal)).All(record => record.Level == TrackerDiagnosticLevel.Debug), "recipe-grid treatment diagnostics use Debug level");
Check(treatmentDiagnostics.Records.Where(record => record.Message.StartsWith("recipe-grid-treatment action=", StringComparison.Ordinal)).All(record => record.Message.Length < 128), "recipe-grid treatment diagnostics remain bounded");

var missingTreatmentDiagnostics = new RecordingDiagnosticSink();
var missingTreatmentAdapter = new RecordingRecipeGridTreatmentAdapter { InitializeResult = false };
using (var treatment = new RecipeGridTreatment(missingTreatmentAdapter, missingTreatmentDiagnostics))
{
    Check(!treatment.TryInitialize(), "missing recipe-grid treatment resources fail softly");
    Check(!treatment.IsAvailable, "missing recipe-grid treatment remains unavailable");
}
Check(missingTreatmentAdapter.ReleaseCalls == 1, "partial recipe-grid treatment cleanup is one-time");
Check(missingTreatmentDiagnostics.Records.Count(record => record.Message.Contains("action=disable", StringComparison.Ordinal)) == 1, "missing recipe-grid treatment failure is reported once");

var failedTreatmentDiagnostics = new RecordingDiagnosticSink();
var failedTreatmentAdapter = new RecordingRecipeGridTreatmentAdapter { ReadResult = false };
using (var treatment = new RecipeGridTreatment(failedTreatmentAdapter, failedTreatmentDiagnostics))
{
    Check(treatment.TryInitialize(), "failing recipe-grid treatment initializes before isolated failure");
    Check(!treatment.TryRefresh(Array.Empty<int>()), "recipe-grid population failure disables only treatment");
    Check(!treatment.TryRefresh(Array.Empty<int>()), "disabled recipe-grid treatment remains inert");
}
Check(failedTreatmentAdapter.ReleaseCalls == 1, "failed recipe-grid treatment cleanup is one-time");
Check(failedTreatmentDiagnostics.Records.Count(record => record.Message.Contains("action=disable", StringComparison.Ordinal)) == 1, "recipe-grid treatment failure is reported once");

Check(PanelGeometry.FixedWidth == 360f, "fixed panel width");
Check(PanelGeometry.FixedHeight == 252f, "fixed panel height");

var parent = new ParentBounds(0f, 0f, 1280f, 720f);
var panel = PanelGeometry.Create(400f, 200f);
CheckRect(PanelGeometry.MoveAndClamp(panel, new DragDelta(-1000f, 0f), parent), 0f, 200f, "left edge");
CheckRect(PanelGeometry.MoveAndClamp(panel, new DragDelta(1000f, 0f), parent), 920f, 200f, "right edge");
CheckRect(PanelGeometry.MoveAndClamp(panel, new DragDelta(0f, -1000f), parent), 400f, 0f, "top edge");
CheckRect(PanelGeometry.MoveAndClamp(panel, new DragDelta(0f, 1000f), parent), 400f, 468f, "bottom edge");
CheckRect(PanelGeometry.MoveAndClamp(panel, new DragDelta(-1000f, -1000f), parent), 0f, 0f, "top-left corner");
CheckRect(PanelGeometry.MoveAndClamp(panel, new DragDelta(1000f, -1000f), parent), 920f, 0f, "top-right corner");
CheckRect(PanelGeometry.MoveAndClamp(panel, new DragDelta(-1000f, 1000f), parent), 0f, 468f, "bottom-left corner");
CheckRect(PanelGeometry.MoveAndClamp(panel, new DragDelta(1000f, 1000f), parent), 920f, 468f, "bottom-right corner");

var repeatedlyMoved = PanelGeometry.Create(100f, 100f);
repeatedlyMoved = PanelGeometry.MoveAndClamp(repeatedlyMoved, new DragDelta(300f, 200f), parent);
repeatedlyMoved = PanelGeometry.MoveAndClamp(repeatedlyMoved, new DragDelta(300f, 200f), parent);
repeatedlyMoved = PanelGeometry.MoveAndClamp(repeatedlyMoved, new DragDelta(300f, 200f), parent);
CheckRect(repeatedlyMoved, 920f, 468f, "repeated drag deltas");

var resizedParent = new ParentBounds(20f, 30f, 800f, 500f);
CheckRect(PanelGeometry.Clamp(repeatedlyMoved, resizedParent), 460f, 278f, "parent-size change");

var undersizedParent = new ParentBounds(20f, 30f, 300f, 200f);
CheckRect(PanelGeometry.Clamp(panel, undersizedParent), 20f, 30f, "undersized parent anchors panel origin");

var visibilityCases = new[]
{
    (HasRows: false, ManualRequested: false, MajorInterfaceActive: false, Expected: false),
    (HasRows: false, ManualRequested: false, MajorInterfaceActive: true, Expected: false),
    (HasRows: false, ManualRequested: true, MajorInterfaceActive: false, Expected: false),
    (HasRows: false, ManualRequested: true, MajorInterfaceActive: true, Expected: false),
    (HasRows: true, ManualRequested: false, MajorInterfaceActive: false, Expected: false),
    (HasRows: true, ManualRequested: false, MajorInterfaceActive: true, Expected: false),
    (HasRows: true, ManualRequested: true, MajorInterfaceActive: false, Expected: true),
    (HasRows: true, ManualRequested: true, MajorInterfaceActive: true, Expected: false)
};

foreach (var visibilityCase in visibilityCases)
{
    var actual = VisibilityPolicy.IsVisible(
        visibilityCase.HasRows,
        visibilityCase.ManualRequested,
        visibilityCase.MajorInterfaceActive);
    Check(
        actual == visibilityCase.Expected,
        $"visibility truth table ({visibilityCase.HasRows}, {visibilityCase.ManualRequested}, {visibilityCase.MajorInterfaceActive})");
}

var individualMajorInterfaces = new[]
{
    (Name: "Tech", Signals: new MajorInterfaceSignals(true, false, false, false, false, false)),
    (Name: "DysonEditor", Signals: new MajorInterfaceSignals(false, true, false, false, false, false)),
    (Name: "Inventory", Signals: new MajorInterfaceSignals(false, false, true, false, false, false)),
    (Name: "Replicator", Signals: new MajorInterfaceSignals(false, false, false, true, false, false)),
    (Name: "Statistics", Signals: new MajorInterfaceSignals(false, false, false, false, true, false)),
    (Name: "Dashboard", Signals: new MajorInterfaceSignals(false, false, false, false, false, true))
};

foreach (var majorInterface in individualMajorInterfaces)
{
    var diagnostics = new RecordingDiagnosticSink();
    var adapter = new RecordingMajorInterfaceStateAdapter { Signals = majorInterface.Signals };
    var input = new MajorInterfaceVisibilityInput(adapter, diagnostics);
    var snapshot = input.Read();
    Check(snapshot.IsAvailable, majorInterface.Name + " signal is available");
    Check(snapshot.IsActive == true, majorInterface.Name + " signal activates the combined state");
    Check(
        diagnostics.Records.Single().Message.Contains("members=[" + majorInterface.Name + "]", StringComparison.Ordinal),
        majorInterface.Name + " diagnostic names the active member");
}

var combinedAdapter = new RecordingMajorInterfaceStateAdapter
{
    Signals = new MajorInterfaceSignals(true, false, true, false, false, true)
};
var combinedInput = new MajorInterfaceVisibilityInput(combinedAdapter, new RecordingDiagnosticSink());
var combinedSnapshot = combinedInput.Read();
Check(combinedSnapshot.IsAvailable && combinedSnapshot.IsActive == true, "simultaneous major-interface signals combine with logical OR");
Check(combinedSnapshot.Signals.FormatActiveMembers() == "[Tech,Inventory,Dashboard]", "simultaneous signal names use the fixed order");

combinedAdapter.Signals = new MajorInterfaceSignals(false, false, false, false, false, false);
var inactiveSnapshot = combinedInput.Read();
Check(inactiveSnapshot.IsAvailable && inactiveSnapshot.IsActive == false, "all-false signals remain available and inactive");

var unavailableAdapter = new RecordingMajorInterfaceStateAdapter { Available = false };
var unavailableInput = new MajorInterfaceVisibilityInput(unavailableAdapter, new RecordingDiagnosticSink());
var unavailableSnapshot = unavailableInput.Read();
Check(!unavailableSnapshot.IsAvailable, "unavailable bindings remain explicitly unavailable");
Check(!unavailableSnapshot.IsActive.HasValue, "unavailable bindings do not invent an inactive value");
Check(!MajorInterfaceVisibilityInput.ResolveTrackerVisibility(true, true, unavailableSnapshot), "unavailable bindings hide presentation fail-closed");
Check(
    MajorInterfaceVisibilityInput.ResolveTrackerVisibility(true, true, MajorInterfaceSnapshot.Available(default(MajorInterfaceSignals))),
    "an available inactive snapshot preserves manual visibility intent");
Check(
    !MajorInterfaceVisibilityInput.ResolveTrackerVisibility(true, false, MajorInterfaceSnapshot.Available(default(MajorInterfaceSignals))),
    "the collection boundary does not redefine manual visibility policy");

var throwingMajorInterfaceAdapter = new RecordingMajorInterfaceStateAdapter { ThrowOnRead = true };
var throwingMajorInterfaceInput = new MajorInterfaceVisibilityInput(throwingMajorInterfaceAdapter, new RecordingDiagnosticSink());
Check(!throwingMajorInterfaceInput.Read().IsAvailable, "adapter exceptions fail softly as unavailable");

var visibilityDiagnostics = new RecordingDiagnosticSink();
var transitionAdapter = new RecordingMajorInterfaceStateAdapter { Available = false };
var transitionInput = new MajorInterfaceVisibilityInput(transitionAdapter, visibilityDiagnostics);
transitionInput.Read();
transitionInput.Read();
transitionAdapter.Available = true;
transitionAdapter.Signals = default(MajorInterfaceSignals);
transitionInput.Read();
transitionInput.Read();
transitionAdapter.Signals = new MajorInterfaceSignals(true, false, false, false, false, false);
transitionInput.Read();
transitionAdapter.Signals = new MajorInterfaceSignals(false, false, true, false, false, false);
transitionInput.Read();
transitionAdapter.Signals = default(MajorInterfaceSignals);
transitionInput.Read();
transitionAdapter.Available = false;
transitionInput.Read();
transitionInput.Read();
transitionAdapter.Available = true;
transitionAdapter.Signals = new MajorInterfaceSignals(true, false, false, false, false, true);
transitionInput.Read();
Check(visibilityDiagnostics.Records.Count == 6, "major-interface diagnostics suppress unchanged availability and combined state");
Check(visibilityDiagnostics.Records[0].Message == "major-interface availability=unavailable", "initial unavailable binding is diagnosed once");
Check(visibilityDiagnostics.Records[1].Message == "major-interface availability=available active=false members=[]", "available transition reports inactive state");
Check(visibilityDiagnostics.Records[2].Message == "major-interface state=active members=[Tech]", "active transition names its member");
Check(visibilityDiagnostics.Records[3].Message == "major-interface state=inactive members=[]", "inactive transition is diagnosed");
Check(visibilityDiagnostics.Records[4].Message == "major-interface availability=unavailable", "availability loss is diagnosed");
Check(visibilityDiagnostics.Records[5].Message == "major-interface availability=available active=true members=[Tech,Dashboard]", "availability recovery reports combined state and members");
Check(visibilityDiagnostics.Records.All(record => record.Level == TrackerDiagnosticLevel.Debug), "major-interface diagnostics use Debug level");
Check(visibilityDiagnostics.Records.All(record => record.Message.Length < 128), "major-interface diagnostics remain bounded");

var uiAdapter = new RecordingPanelUiAdapter();
using (var uiBoundary = new TrackerPanelUiBoundary(uiAdapter))
{
    Check(uiBoundary.TryInitialize(PanelGeometry.Create(100f, 100f)), "UI boundary initializes");
    Check(uiBoundary.IsAvailable, "UI boundary reports availability");
    Check(uiAdapter.CreateCalls == 1, "UI boundary creates once");
    Check(uiAdapter.RaycastCalls == 1, "UI boundary enables raycast containment once");
    Check(uiBoundary.TryApplyDrag(new DragDelta(2000f, 2000f), parent), "UI boundary accepts drag");
    CheckRect(uiAdapter.LastRectangle, 920f, 468f, "UI boundary applies clamped drag");
    Check(uiBoundary.TryApplyVisibility(true), "UI boundary applies visible result");
    Check(uiAdapter.LastVisibility == true, "UI boundary preserves visible result");
    Check(uiBoundary.TryApplyVisibility(false), "UI boundary applies hidden result");
    Check(uiAdapter.LastVisibility == false, "UI boundary preserves hidden result");
}
Check(uiAdapter.ReleaseCalls == 1, "UI boundary releases once");

var missingAdapter = new RecordingPanelUiAdapter { CreateResult = false };
using (var missingBoundary = new TrackerPanelUiBoundary(missingAdapter))
{
    Check(!missingBoundary.TryInitialize(PanelGeometry.Create(0f, 0f)), "missing UI adapter fails softly");
    Check(!missingBoundary.IsAvailable, "missing UI adapter remains unavailable");
}
Check(missingAdapter.ReleaseCalls == 1, "missing UI adapter release is bounded");

var throwingAdapter = new RecordingPanelUiAdapter { ThrowDuringLayout = true };
using (var throwingBoundary = new TrackerPanelUiBoundary(throwingAdapter))
{
    Check(!throwingBoundary.TryInitialize(PanelGeometry.Create(0f, 0f)), "throwing UI adapter fails softly");
    Check(!throwingBoundary.TryApplyVisibility(true), "failed UI adapter remains inert");
}
Check(throwingAdapter.ReleaseCalls == 1, "throwing UI adapter release is bounded");

if (failures.Count != 0)
{
    foreach (var failure in failures)
    {
        Console.Error.WriteLine("FAIL: " + failure);
    }

    return 1;
}

Console.WriteLine("DSPRecipeTracker deterministic identity, pin input, recipe-grid treatment, major-interface visibility, panel geometry, visibility, and UI boundary tests passed.");
return 0;

void Check(bool condition, string name)
{
    if (!condition)
    {
        failures.Add(name);
    }
}

void CheckRect(PanelRectangle actual, float expectedLeft, float expectedTop, string name)
{
    Check(actual.Left == expectedLeft, name + " left");
    Check(actual.Top == expectedTop, name + " top");
    Check(actual.Width == PanelGeometry.FixedWidth, name + " width");
    Check(actual.Height == PanelGeometry.FixedHeight, name + " height");
}

void CheckRecipeOrder(IReadOnlyList<int> actual, string name, params int[] expected)
{
    Check(actual.Count == expected.Length, name + " count");
    for (var index = 0; index < Math.Min(actual.Count, expected.Length); index++)
    {
        Check(actual[index] == expected[index], name + " index " + index);
    }
}

void CheckTreatmentState(IReadOnlyList<uint> actual, string name, int index, uint expected)
{
    Check(actual.Count == RecipeGridTreatmentModel.CellCount, name + " count");
    if (index < actual.Count)
    {
        Check(actual[index] == expected, name + " state");
    }
}

internal readonly record struct DiagnosticRecord(TrackerDiagnosticLevel Level, string Message);

internal sealed class RecordingDiagnosticSink : ITrackerDiagnosticSink
{
    public List<DiagnosticRecord> Records { get; } = new List<DiagnosticRecord>();

    public void Write(TrackerDiagnosticLevel level, string message)
    {
        Records.Add(new DiagnosticRecord(level, message));
    }
}

internal sealed class RecordingPanelUiAdapter : ITrackerPanelUiAdapter
{
    public bool CreateResult { get; set; } = true;

    public bool ThrowDuringLayout { get; set; }

    public int CreateCalls { get; private set; }

    public int RaycastCalls { get; private set; }

    public int ReleaseCalls { get; private set; }

    public PanelRectangle LastRectangle { get; private set; }

    public bool? LastVisibility { get; private set; }

    public bool TryCreate()
    {
        CreateCalls++;
        return CreateResult;
    }

    public bool TryApplyLayout(PanelRectangle rectangle)
    {
        if (ThrowDuringLayout)
        {
            throw new InvalidOperationException("Unavailable layout member.");
        }

        LastRectangle = rectangle;
        return true;
    }

    public bool TryEnableRaycastContainment()
    {
        RaycastCalls++;
        return true;
    }

    public bool TryApplyVisibility(bool visible)
    {
        LastVisibility = visible;
        return true;
    }

    public void Release()
    {
        ReleaseCalls++;
    }
}

internal sealed class RecordingReplicatorPinInputAdapter : IReplicatorPinInputAdapter
{
    private Action<ReplicatorPointerButton>? listener;

    public bool AttachResult { get; set; } = true;

    public int CurrentRecipeIndex { get; set; }

    public int?[] RecipeIds { get; set; } = Array.Empty<int?>();

    public int AttachCalls { get; private set; }

    public int ReleaseCalls { get; private set; }

    public int NativeSelectionCalls { get; private set; }

    public bool NativeSelectionObservedBeforeTracker { get; private set; }

    public int ListenerCount => listener == null ? 0 : 1;

    public bool TryAttach(Action<ReplicatorPointerButton> pointerDown)
    {
        AttachCalls++;
        if (!AttachResult)
        {
            return false;
        }

        listener = pointerDown;
        return true;
    }

    public bool TryGetCurrentRecipe(out int gridIndex, out int recipeId)
    {
        gridIndex = CurrentRecipeIndex;
        NativeSelectionObservedBeforeTracker = NativeSelectionCalls > 0;
        if (CurrentRecipeIndex < 0 || CurrentRecipeIndex >= RecipeIds.Length || !RecipeIds[CurrentRecipeIndex].HasValue)
        {
            recipeId = 0;
            return false;
        }

        recipeId = RecipeIds[CurrentRecipeIndex]!.Value;
        return true;
    }

    public void Release()
    {
        ReleaseCalls++;
        listener = null;
    }

    public void Raise(ReplicatorPointerButton button)
    {
        NativeSelectionCalls++;
        listener?.Invoke(button);
    }
}

internal sealed class RecordingRecipeGridTreatmentAdapter : IRecipeGridTreatmentAdapter
{
    private readonly int[] population = new int[RecipeGridTreatmentModel.CellCount];

    public bool InitializeResult { get; set; } = true;

    public bool ReadResult { get; set; } = true;

    public bool ApplyResult { get; set; } = true;

    public int InitializeCalls { get; private set; }

    public int ApplyCalls { get; private set; }

    public int ReleaseCalls { get; private set; }

    public uint[] OriginalState { get; } = Enumerable.Repeat(99u, RecipeGridTreatmentModel.CellCount).ToArray();

    public uint[] LastAppliedState { get; private set; } = new uint[RecipeGridTreatmentModel.CellCount];

    public bool TryInitialize()
    {
        InitializeCalls++;
        return InitializeResult;
    }

    public bool TryReadPopulation(int[] recipeIds)
    {
        if (!ReadResult)
        {
            return false;
        }

        Array.Copy(population, recipeIds, population.Length);
        return true;
    }

    public bool TryApplyState(uint[] states)
    {
        if (!ApplyResult)
        {
            return false;
        }

        ApplyCalls++;
        LastAppliedState = (uint[])states.Clone();
        return true;
    }

    public void Release()
    {
        ReleaseCalls++;
    }

    public void SetPopulation(params int[] recipeIds)
    {
        Array.Clear(population, 0, population.Length);
        Array.Copy(recipeIds, population, Math.Min(recipeIds.Length, population.Length));
    }
}

internal sealed class RecordingMajorInterfaceStateAdapter : IMajorInterfaceStateAdapter
{
    public bool Available { get; set; } = true;

    public bool ThrowOnRead { get; set; }

    public MajorInterfaceSignals Signals { get; set; }

    public bool TryRead(out MajorInterfaceSignals signals)
    {
        if (ThrowOnRead)
        {
            throw new InvalidOperationException("Unavailable major-interface binding.");
        }

        signals = Signals;
        return Available;
    }
}
