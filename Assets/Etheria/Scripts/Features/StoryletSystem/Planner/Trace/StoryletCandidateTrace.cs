using System;

namespace Etheria.Features.StoryletSystem
{
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
