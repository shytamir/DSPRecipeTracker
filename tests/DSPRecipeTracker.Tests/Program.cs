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

Console.WriteLine("DSPRecipeTracker deterministic identity, panel geometry, visibility, and UI boundary tests passed.");
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
