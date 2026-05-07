using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletPlannerTrace
    {
        public StoryletPlannerTrace(
            IReadOnlyList<StoryletPlannerStepTrace> stepTraces,
            IReadOnlyList<StoryletBeamExpansionTrace> beamExpansions)
        {
            StepTraces = stepTraces ?? throw new ArgumentNullException(nameof(stepTraces));
            BeamExpansions = beamExpansions ?? throw new ArgumentNullException(nameof(beamExpansions));
        }

        public IReadOnlyList<StoryletPlannerStepTrace> StepTraces { get; }
        public IReadOnlyList<StoryletBeamExpansionTrace> BeamExpansions { get; }
    }
}
