using System;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletPlannedStep
    {
        public StoryletPlannedStep(
            int stepNumber,
            StoryletWorldState beforeWorldState,
            StoryletInstantiationCandidate candidate,
            StoryletWorldState afterWorldState)
        {
            StepNumber = stepNumber;
            BeforeWorldState = beforeWorldState ?? throw new ArgumentNullException(nameof(beforeWorldState));
            Candidate = candidate ?? throw new ArgumentNullException(nameof(candidate));
            AfterWorldState = afterWorldState ?? throw new ArgumentNullException(nameof(afterWorldState));
        }

        public int StepNumber { get; }
        public StoryletWorldState BeforeWorldState { get; }
        public StoryletInstantiationCandidate Candidate { get; }
        public StoryletWorldState AfterWorldState { get; }
    }
}
