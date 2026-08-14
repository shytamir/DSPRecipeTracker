using DSPRecipeTracker;

var failures = new List<string>();

Check(PluginMetadata.Guid == "dsprecipetracker", "plugin GUID");
Check(PluginMetadata.DisplayName == "DSP-Recipe-Tracker", "plugin display name");
Check(BuildIdentity.SemanticVersion == $"{BuildIdentity.Major}.{BuildIdentity.Minor}.{BuildIdentity.Build}", "semantic version");
Check(BuildIdentity.AssemblyVersion == BuildIdentity.SemanticVersion + ".0", "assembly version");
Check(BuildIdentity.DiagnosticLabel.StartsWith(BuildIdentity.SemanticVersion + ".", StringComparison.Ordinal), "diagnostic label");
Check(BuildIdentity.DiagnosticLabel != BuildIdentity.SemanticVersion, "diagnostic label is not loader identity");

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

if (failures.Count != 0)
{
    foreach (var failure in failures)
    {
        Console.Error.WriteLine("FAIL: " + failure);
    }

    return 1;
}

Console.WriteLine("DSPRecipeTracker deterministic identity, panel geometry, and visibility tests passed.");
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
