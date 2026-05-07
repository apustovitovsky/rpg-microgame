using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletPlannerStepTrace
    {
        public StoryletPlannerStepTrace(
            int stepNumber,
            int currentSnapshotId,
            IReadOnlyList<StoryletCandidateTrace> candidates,
            IReadOnlyList<StoryletBeamBranchTrace> survivingBranches,
            StoryletPlannedStep selectedStep)
        {
            StepNumber = stepNumber;
            CurrentSnapshotId = currentSnapshotId;
            Candidates = candidates ?? throw new ArgumentNullException(nameof(candidates));
            SurvivingBranches = survivingBranches ?? throw new ArgumentNullException(nameof(survivingBranches));
            SelectedStep = selectedStep;
        }

        public int StepNumber { get; }
        public int CurrentSnapshotId { get; }
        public IReadOnlyList<StoryletCandidateTrace> Candidates { get; }
        public IReadOnlyList<StoryletBeamBranchTrace> SurvivingBranches { get; }
        public StoryletPlannedStep SelectedStep { get; }
    }
}
