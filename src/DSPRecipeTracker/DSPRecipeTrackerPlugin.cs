using BepInEx;

namespace DSPRecipeTracker
{
    [BepInPlugin(PluginMetadata.Guid, PluginMetadata.DisplayName, BuildIdentity.SemanticVersion)]
    public sealed class DSPRecipeTrackerPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo(PluginMetadata.DisplayName + " " + BuildIdentity.DiagnosticLabel + " loaded.");
        }
    }
}
