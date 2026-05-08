using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletDiagnosticsResult
    {
        public StoryletDiagnosticsResult(StoryletPlannerTrace trace)
        {
            Trace = trace ?? throw new ArgumentNullException(nameof(trace));
        }

        public StoryletPlannerTrace Trace { get; }
        public IReadOnlyList<StoryletPlannerStepTrace> StepTraces => Trace.StepTraces;
        public IReadOnlyList<StoryletBeamExpansionTrace> BeamExpansions => Trace.BeamExpansions;
    }
}
