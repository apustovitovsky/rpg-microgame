using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletSimulationRequest
    {
        public StoryletSimulationRequest(
            StoryletWorldState initialWorldState,
            IReadOnlyList<StoryletDefinition> storylets,
            StoryletPlannerMemory initialMemory = null,
            bool includeDiagnostics = true,
            StoryletPlannerOptions plannerOptions = null)
        {
            InitialWorldState = initialWorldState ?? throw new ArgumentNullException(nameof(initialWorldState));
            Storylets = storylets ?? throw new ArgumentNullException(nameof(storylets));
            InitialMemory = initialMemory ?? StoryletPlannerMemory.Empty;
            IncludeDiagnostics = includeDiagnostics;
            PlannerOptions = plannerOptions;
        }

        public StoryletWorldState InitialWorldState { get; }
        public IReadOnlyList<StoryletDefinition> Storylets { get; }
        public StoryletPlannerMemory InitialMemory { get; }
        public bool IncludeDiagnostics { get; }
        public StoryletPlannerOptions PlannerOptions { get; }
    }
}
