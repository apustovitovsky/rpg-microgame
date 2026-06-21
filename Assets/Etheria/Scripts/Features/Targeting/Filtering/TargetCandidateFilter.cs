namespace Etheria.Features.Targeting
{
    public interface ITargetCandidateFilter
    {
        bool IsAllowed(TargetCandidate candidate);
    }

    public sealed class TargetCandidateFilter : ITargetCandidateFilter
    {
        private readonly ITargetEligibilityService _eligibilityService;

        public TargetCandidateFilter(ITargetEligibilityService eligibilityService)
        {
            _eligibilityService = eligibilityService;
        }

        public bool IsAllowed(TargetCandidate candidate)
        {
            return _eligibilityService.IsEligible(candidate);
        }
    }
}
