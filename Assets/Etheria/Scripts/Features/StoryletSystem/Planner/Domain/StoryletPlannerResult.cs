using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletPlannerResult
    {
        public StoryletPlannerResult(
            IReadOnlyList<StoryletPlannedStep> winningSteps,
            StoryletPlannerTrace trace)
        {
            WinningSteps = winningSteps ?? throw new ArgumentNullException(nameof(winningSteps));
            Trace = trace ?? throw new ArgumentNullException(nameof(trace));
        }

        public IReadOnlyList<StoryletPlannedStep> WinningSteps { get; }
        public StoryletPlannerTrace Trace { get; }
    }
}
