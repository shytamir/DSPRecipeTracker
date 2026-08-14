using BepInEx;
using System;
using UnityEngine;

namespace DSPRecipeTracker
{
    [BepInPlugin(PluginMetadata.Guid, PluginMetadata.DisplayName, BuildIdentity.SemanticVersion)]
    public sealed class DSPRecipeTrackerPlugin : BaseUnityPlugin
    {
        private TrackerOrchestrator orchestrator;
        private bool initializationAttempted;

        private void Awake()
        {
            Logger.LogInfo(PluginMetadata.DisplayName + " " + BuildIdentity.DiagnosticLabel + " loaded.");
        }

        private void Update()
        {
            if (ReferenceEquals(orchestrator, null))
            {
                TryInitialize();
            }

            orchestrator?.Refresh();
        }

        private void OnDestroy()
        {
            orchestrator?.Dispose();
            orchestrator = null;
        }

        private void TryInitialize()
        {
            if (initializationAttempted)
            {
                return;
            }

            var root = UIRoot.instance;
            var uiGame = ReferenceEquals(root, null) ? null : root.uiGame;
            if (ReferenceEquals(uiGame, null))
            {
                return;
            }

            initializationAttempted = true;
            try
            {
                var diagnostics = new BepInExTrackerDiagnosticSink(Logger);
                var state = new PinnedRecipeState(diagnostics);
                var panelParent = uiGame.transform as RectTransform;
                var backgroundSprite = ReferenceEquals(uiGame.replicator, null) ||
                    ReferenceEquals(uiGame.replicator.recipeBg, null)
                    ? null
                    : uiGame.replicator.recipeBg.sprite;
                var panelAdapter = new UnityTrackerPanelAdapter(panelParent, backgroundSprite);
                var panel = new TrackerPanelUiBoundary(panelAdapter);
                var pinInput = new ReplicatorPinInput(
                    new UnityReplicatorPinInputAdapter(uiGame.replicator),
                    state,
                    diagnostics);
                var treatment = new RecipeGridTreatment(
                    new UnityRecipeGridTreatmentAdapter(uiGame.replicator),
                    diagnostics);
                var icons = new RecipeIconSlotPresentation(
                    state,
                    new UnityRecipeIconResolver(),
                    panel,
                    diagnostics);
                var majorInterfaces = new MajorInterfaceVisibilityInput(
                    new UnityMajorInterfaceStateAdapter(uiGame),
                    diagnostics);

                var gameMenu = uiGame.gameMenu;
                var globalParent = ReferenceEquals(gameMenu, null)
                    ? null
                    : gameMenu.transform as RectTransform;
                var template = ReferenceEquals(gameMenu, null) ? null : gameMenu.buttonS;
                var nativeIcon = ReferenceEquals(gameMenu, null)
                    ? null
                    : UnityTrackerVisibilityControlAdapter.TryResolveNativeIcon(gameMenu.button3);
                var controls = new TrackerVisibilityControls(
                    new UnityTrackerVisibilityControlAdapter(
                        globalParent,
                        template,
                        nativeIcon,
                        panelAdapter));

                orchestrator = new TrackerOrchestrator(
                    state,
                    pinInput,
                    treatment,
                    icons,
                    majorInterfaces,
                    panel,
                    controls,
                    diagnostics);
                orchestrator.TryInitialize(PanelGeometry.Create(24f, 84f));
            }
            catch (Exception)
            {
                orchestrator?.Dispose();
                orchestrator = null;
                Logger.LogDebug("tracker-orchestration action=disable stage=startup");
            }
        }
    }
}
