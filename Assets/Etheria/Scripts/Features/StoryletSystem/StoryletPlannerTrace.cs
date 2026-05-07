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

    public sealed class StoryletBeamExpansionTrace
    {
        public StoryletBeamExpansionTrace(
            int depth,
            int beamWidth,
            IReadOnlyList<StoryletBeamBranchTrace> expandedBranches,
            IReadOnlyList<StoryletBeamBranchTrace> keptBranches,
            IReadOnlyList<StoryletBeamBranchTrace> prunedBranches)
        {
            Depth = depth;
            BeamWidth = beamWidth;
            ExpandedBranches = expandedBranches ?? throw new ArgumentNullException(nameof(expandedBranches));
            KeptBranches = keptBranches ?? throw new ArgumentNullException(nameof(keptBranches));
            PrunedBranches = prunedBranches ?? throw new ArgumentNullException(nameof(prunedBranches));
        }

        public int Depth { get; }
        public int BeamWidth { get; }
        public IReadOnlyList<StoryletBeamBranchTrace> ExpandedBranches { get; }
        public IReadOnlyList<StoryletBeamBranchTrace> KeptBranches { get; }
        public IReadOnlyList<StoryletBeamBranchTrace> PrunedBranches { get; }
    }

    public sealed class StoryletBeamBranchTrace
    {
        public StoryletBeamBranchTrace(
            string branchId,
            float totalScore,
            int snapshotId,
            string reason,
            string storyletKey)
        {
            BranchId = branchId ?? throw new ArgumentNullException(nameof(branchId));
            TotalScore = totalScore;
            SnapshotId = snapshotId;
            Reason = reason ?? string.Empty;
            StoryletKey = storyletKey ?? string.Empty;
        }

        public string BranchId { get; }
        public float TotalScore { get; }
        public int SnapshotId { get; }
        public string Reason { get; }
        public string StoryletKey { get; }
    }

    public sealed class StoryletCandidateTrace
    {
        public StoryletCandidateTrace(
            string storyletKey,
            bool isValid,
            string selectionStatus,
            string assignmentSummary,
            StoryletScoreBreakdown scoreBreakdown,
            StoryletRejectionReason rejectionReason)
        {
            StoryletKey = storyletKey ?? throw new ArgumentNullException(nameof(storyletKey));
            IsValid = isValid;
            SelectionStatus = selectionStatus ?? throw new ArgumentNullException(nameof(selectionStatus));
            AssignmentSummary = assignmentSummary ?? string.Empty;
            ScoreBreakdown = scoreBreakdown;
            RejectionReason = rejectionReason;
        }

        public string StoryletKey { get; }
        public bool IsValid { get; }
        public string SelectionStatus { get; }
        public string AssignmentSummary { get; }
        public StoryletScoreBreakdown ScoreBreakdown { get; }
        public StoryletRejectionReason RejectionReason { get; }
    }
}
