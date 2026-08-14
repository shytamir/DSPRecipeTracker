using System;

namespace DSPRecipeTracker
{
    internal struct MajorInterfaceSignals
    {
        public MajorInterfaceSignals(
            bool tech,
            bool dysonEditor,
            bool inventory,
            bool replicator,
            bool statistics,
            bool dashboard)
        {
            Tech = tech;
            DysonEditor = dysonEditor;
            Inventory = inventory;
            Replicator = replicator;
            Statistics = statistics;
            Dashboard = dashboard;
        }

        public bool Tech { get; }

        public bool DysonEditor { get; }

        public bool Inventory { get; }

        public bool Replicator { get; }

        public bool Statistics { get; }

        public bool Dashboard { get; }

        public bool AnyActive =>
            Tech || DysonEditor || Inventory || Replicator || Statistics || Dashboard;

        public string FormatActiveMembers()
        {
            var members = string.Empty;
            AppendMember(ref members, Tech, "Tech");
            AppendMember(ref members, DysonEditor, "DysonEditor");
            AppendMember(ref members, Inventory, "Inventory");
            AppendMember(ref members, Replicator, "Replicator");
            AppendMember(ref members, Statistics, "Statistics");
            AppendMember(ref members, Dashboard, "Dashboard");
            return "[" + members + "]";
        }

        private static void AppendMember(ref string members, bool active, string name)
        {
            if (!active)
            {
                return;
            }

            if (members.Length != 0)
            {
                members += ",";
            }

            members += name;
        }
    }

    internal struct MajorInterfaceSnapshot
    {
        private MajorInterfaceSnapshot(bool isAvailable, MajorInterfaceSignals signals)
        {
            IsAvailable = isAvailable;
            Signals = signals;
        }

        public bool IsAvailable { get; }

        public MajorInterfaceSignals Signals { get; }

        public bool? IsActive => IsAvailable ? Signals.AnyActive : (bool?)null;

        public static MajorInterfaceSnapshot Available(MajorInterfaceSignals signals)
        {
            return new MajorInterfaceSnapshot(true, signals);
        }

        public static MajorInterfaceSnapshot Unavailable()
        {
            return new MajorInterfaceSnapshot(false, default(MajorInterfaceSignals));
        }
    }

    internal interface IMajorInterfaceStateAdapter
    {
        bool TryRead(out MajorInterfaceSignals signals);
    }

    internal sealed class MajorInterfaceVisibilityInput
    {
        private readonly IMajorInterfaceStateAdapter adapter;
        private readonly ITrackerDiagnosticSink diagnostics;
        private bool hasAvailabilityObservation;
        private bool wasAvailable;
        private bool hasActiveObservation;
        private bool wasActive;

        public MajorInterfaceVisibilityInput(
            IMajorInterfaceStateAdapter adapter,
            ITrackerDiagnosticSink diagnostics)
        {
            this.adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            this.diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        }

        public MajorInterfaceSnapshot Read()
        {
            MajorInterfaceSignals signals;
            bool available;
            try
            {
                available = adapter.TryRead(out signals);
            }
            catch
            {
                available = false;
                signals = default(MajorInterfaceSignals);
            }

            if (!available)
            {
                if (!hasAvailabilityObservation || wasAvailable)
                {
                    diagnostics.Write(
                        TrackerDiagnosticLevel.Debug,
                        "major-interface availability=unavailable");
                }

                hasAvailabilityObservation = true;
                wasAvailable = false;
                hasActiveObservation = false;
                return MajorInterfaceSnapshot.Unavailable();
            }

            var active = signals.AnyActive;
            if (!hasAvailabilityObservation || !wasAvailable)
            {
                diagnostics.Write(
                    TrackerDiagnosticLevel.Debug,
                    "major-interface availability=available active=" +
                    active.ToString().ToLowerInvariant() +
                    " members=" + signals.FormatActiveMembers());
            }
            else if (!hasActiveObservation || wasActive != active)
            {
                diagnostics.Write(
                    TrackerDiagnosticLevel.Debug,
                    "major-interface state=" + (active ? "active" : "inactive") +
                    " members=" + signals.FormatActiveMembers());
            }

            hasAvailabilityObservation = true;
            wasAvailable = true;
            hasActiveObservation = true;
            wasActive = active;
            return MajorInterfaceSnapshot.Available(signals);
        }

        public static bool ResolveTrackerVisibility(
            bool hasRows,
            bool manualRequested,
            MajorInterfaceSnapshot snapshot)
        {
            if (!snapshot.IsAvailable)
            {
                return false;
            }

            return VisibilityPolicy.IsVisible(
                hasRows,
                manualRequested,
                snapshot.Signals.AnyActive);
        }
    }
}
