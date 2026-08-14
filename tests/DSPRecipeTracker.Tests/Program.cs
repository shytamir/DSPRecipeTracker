using DSPRecipeTracker;

var failures = new List<string>();

Check(PluginMetadata.Guid == "dsprecipetracker", "plugin GUID");
Check(PluginMetadata.DisplayName == "DSP-Recipe-Tracker", "plugin display name");
Check(BuildIdentity.SemanticVersion == $"{BuildIdentity.Major}.{BuildIdentity.Minor}.{BuildIdentity.Build}", "semantic version");
Check(BuildIdentity.AssemblyVersion == BuildIdentity.SemanticVersion + ".0", "assembly version");
Check(BuildIdentity.DiagnosticLabel.StartsWith(BuildIdentity.SemanticVersion + ".", StringComparison.Ordinal), "diagnostic label");
Check(BuildIdentity.DiagnosticLabel != BuildIdentity.SemanticVersion, "diagnostic label is not loader identity");

if (failures.Count != 0)
{
    foreach (var failure in failures)
    {
        Console.Error.WriteLine("FAIL: " + failure);
    }

    return 1;
}

Console.WriteLine("DSPRecipeTracker deterministic identity tests passed.");
return 0;

void Check(bool condition, string name)
{
    if (!condition)
    {
        failures.Add(name);
    }
}
