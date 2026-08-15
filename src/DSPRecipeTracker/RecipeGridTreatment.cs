using System;
using System.Collections.Generic;

namespace DSPRecipeTracker
{
    internal readonly struct RecipeGridTreatmentRefresh
    {
        public RecipeGridTreatmentRefresh(bool changed, int populatedCount, int unpinnedCount, int pinnedCount)
        {
            Changed = changed;
            PopulatedCount = populatedCount;
            UnpinnedCount = unpinnedCount;
            PinnedCount = pinnedCount;
        }

        public bool Changed { get; }

        public int PopulatedCount { get; }

        public int UnpinnedCount { get; }

        public int PinnedCount { get; }
    }

    internal sealed class RecipeGridTreatmentModel
    {
        public const int CellCount = 120;
        public const uint NeutralMask = 0x0;
        public const uint PinnedMarkerState = 0x1;

        private readonly int[] previousPopulation = new int[CellCount];
        private readonly int[] previousPins = new int[PinnedRecipeState.Capacity];
        private readonly uint[] states = new uint[CellCount];
        private int previousPinCount;
        private bool hasSnapshot;

        public uint[] States => states;

        public RecipeGridTreatmentRefresh Refresh(
            IReadOnlyList<int> population,
            IReadOnlyList<int> pinnedRecipeIds)
        {
            if (population == null)
            {
                throw new ArgumentNullException(nameof(population));
            }

            if (pinnedRecipeIds == null)
            {
                throw new ArgumentNullException(nameof(pinnedRecipeIds));
            }

            if (population.Count != CellCount)
            {
                throw new ArgumentException("The Replicator population must contain exactly 120 cells.", nameof(population));
            }

            if (pinnedRecipeIds.Count > PinnedRecipeState.Capacity)
            {
                throw new ArgumentException("The treatment cannot consume more than three pins.", nameof(pinnedRecipeIds));
            }

            var changed = !hasSnapshot || PopulationChanged(population) || PinsChanged(pinnedRecipeIds);
            if (!changed)
            {
                return new RecipeGridTreatmentRefresh(false, 0, 0, 0);
            }

            var populatedCount = 0;
            var unpinnedCount = 0;
            var pinnedCount = 0;
            for (var index = 0; index < CellCount; index++)
            {
                var recipeId = population[index];
                previousPopulation[index] = recipeId;
                if (recipeId <= 0)
                {
                    states[index] = NeutralMask;
                    continue;
                }

                populatedCount++;
                if (Contains(pinnedRecipeIds, recipeId))
                {
                    states[index] = PinnedMarkerState;
                    pinnedCount++;
                }
                else
                {
                    states[index] = NeutralMask;
                    unpinnedCount++;
                }
            }

            previousPinCount = pinnedRecipeIds.Count;
            for (var index = 0; index < previousPins.Length; index++)
            {
                previousPins[index] = index < previousPinCount ? pinnedRecipeIds[index] : 0;
            }

            hasSnapshot = true;
            return new RecipeGridTreatmentRefresh(true, populatedCount, unpinnedCount, pinnedCount);
        }

        private bool PopulationChanged(IReadOnlyList<int> population)
        {
            for (var index = 0; index < CellCount; index++)
            {
                if (previousPopulation[index] != population[index])
                {
                    return true;
                }
            }

            return false;
        }

        private bool PinsChanged(IReadOnlyList<int> pinnedRecipeIds)
        {
            if (previousPinCount != pinnedRecipeIds.Count)
            {
                return true;
            }

            for (var index = 0; index < previousPinCount; index++)
            {
                if (previousPins[index] != pinnedRecipeIds[index])
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Contains(IReadOnlyList<int> recipeIds, int recipeId)
        {
            for (var index = 0; index < recipeIds.Count; index++)
            {
                if (recipeIds[index] == recipeId)
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal interface IRecipeGridTreatmentAdapter
    {
        bool TryInitialize();

        bool TryReadPopulation(int[] recipeIds);

        bool TryApplyState(uint[] states);

        void Release();
    }

    internal sealed class RecipeGridTreatment : IDisposable
    {
        private readonly IRecipeGridTreatmentAdapter adapter;
        private readonly ITrackerDiagnosticSink diagnostics;
        private readonly RecipeGridTreatmentModel model = new RecipeGridTreatmentModel();
        private readonly int[] population = new int[RecipeGridTreatmentModel.CellCount];
        private bool available;
        private bool initialized;
        private bool released;
        private bool failureReported;

        public RecipeGridTreatment(IRecipeGridTreatmentAdapter adapter, ITrackerDiagnosticSink diagnostics)
        {
            this.adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            this.diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        }

        public bool IsAvailable => available;

        public bool TryInitialize()
        {
            if (released || available)
            {
                return false;
            }

            try
            {
                if (!adapter.TryInitialize())
                {
                    return FailSoftly("initialize");
                }

                initialized = true;
                available = true;
                diagnostics.Write(
                    TrackerDiagnosticLevel.Debug,
                    "recipe-grid-treatment action=initialize cells=120 markers=3 style=green-corners");
                return true;
            }
            catch (Exception)
            {
                return FailSoftly("initialize");
            }
        }

        public bool TryRefresh(IReadOnlyList<int> pinnedRecipeIds)
        {
            if (!available)
            {
                return false;
            }

            try
            {
                if (!adapter.TryReadPopulation(population))
                {
                    return FailSoftly("population");
                }

                var refresh = model.Refresh(population, pinnedRecipeIds);
                if (!refresh.Changed)
                {
                    return true;
                }

                if (!adapter.TryApplyState(model.States))
                {
                    return FailSoftly("apply");
                }

                diagnostics.Write(
                    TrackerDiagnosticLevel.Debug,
                    "recipe-grid-treatment action=refresh populated=" + refresh.PopulatedCount +
                    " unpinned=" + refresh.UnpinnedCount +
                    " pinned=" + refresh.PinnedCount);
                return true;
            }
            catch (Exception)
            {
                return FailSoftly("refresh");
            }
        }

        public void Dispose()
        {
            ReleaseAdapter();
        }

        private bool FailSoftly(string stage)
        {
            if (!failureReported)
            {
                diagnostics.Write(
                    TrackerDiagnosticLevel.Debug,
                    "recipe-grid-treatment action=disable stage=" + stage);
                failureReported = true;
            }

            ReleaseAdapter();
            return false;
        }

        private void ReleaseAdapter()
        {
            if (released)
            {
                return;
            }

            available = false;
            released = true;
            try
            {
                adapter.Release();
            }
            catch (Exception)
            {
                // Cleanup remains best-effort after a missing or changed game resource.
            }

            if (initialized)
            {
                diagnostics.Write(TrackerDiagnosticLevel.Debug, "recipe-grid-treatment action=release");
                initialized = false;
            }
        }
    }
}
