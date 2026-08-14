using BepInEx.Logging;

namespace DSPRecipeTracker
{
    internal sealed class BepInExTrackerDiagnosticSink : ITrackerDiagnosticSink
    {
        private readonly ManualLogSource logger;

        public BepInExTrackerDiagnosticSink(ManualLogSource logger)
        {
            this.logger = logger;
        }

        public void Write(TrackerDiagnosticLevel level, string message)
        {
            logger.LogDebug(message);
        }
    }
}
