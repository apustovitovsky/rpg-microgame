namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletInstantiationResult
    {
        public StoryletInstantiationResult(
            bool isValid,
            StoryletInstantiationCandidate candidate,
            StoryletRejectionReason rejectionReason)
        {
            IsValid = isValid;
            Candidate = candidate;
            RejectionReason = rejectionReason;
        }

        public bool IsValid { get; }
        public StoryletInstantiationCandidate Candidate { get; }
        public StoryletRejectionReason RejectionReason { get; }
    }
}
