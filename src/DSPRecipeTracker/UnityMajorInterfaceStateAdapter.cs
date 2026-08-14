using System;

namespace DSPRecipeTracker
{
    internal sealed class UnityMajorInterfaceStateAdapter : IMajorInterfaceStateAdapter
    {
        private readonly ManualBehaviour tech;
        private readonly ManualBehaviour dysonEditor;
        private readonly ManualBehaviour inventory;
        private readonly ManualBehaviour replicator;
        private readonly ManualBehaviour statistics;
        private readonly ManualBehaviour dashboard;
        private readonly bool bindingsAvailable;

        public UnityMajorInterfaceStateAdapter(UIGame uiGame)
        {
            if (ReferenceEquals(uiGame, null))
            {
                return;
            }

            tech = uiGame.techTree;
            dysonEditor = uiGame.dysonEditor;
            inventory = uiGame.inventoryWindow;
            replicator = uiGame.replicator;
            statistics = uiGame.statWindow;
            dashboard = uiGame.dashboard;
            bindingsAvailable =
                !ReferenceEquals(tech, null) &&
                !ReferenceEquals(dysonEditor, null) &&
                !ReferenceEquals(inventory, null) &&
                !ReferenceEquals(replicator, null) &&
                !ReferenceEquals(statistics, null) &&
                !ReferenceEquals(dashboard, null);
        }

        public bool TryRead(out MajorInterfaceSignals signals)
        {
            signals = default(MajorInterfaceSignals);
            if (!bindingsAvailable)
            {
                return false;
            }

            try
            {
                signals = new MajorInterfaceSignals(
                    tech.active,
                    dysonEditor.active,
                    inventory.active,
                    replicator.active,
                    statistics.active,
                    dashboard.active);
                return true;
            }
            catch
            {
                signals = default(MajorInterfaceSignals);
                return false;
            }
        }
    }
}
