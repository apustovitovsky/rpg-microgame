using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public sealed class TargetValidator : ITargetValidator
    {
        private readonly ITargetCandidateResolver _candidateResolver;
        private readonly ITargetLineOfSightChecker _lineOfSightChecker;
        private readonly ITargetEligibilityService _eligibilityService;

        public TargetValidator(
            ITargetCandidateResolver candidateResolver,
            ITargetLineOfSightChecker lineOfSightChecker,
            ITargetEligibilityService eligibilityService)
        {
            _candidateResolver = candidateResolver;
            _lineOfSightChecker = lineOfSightChecker;
            _eligibilityService = eligibilityService;
        }

        public bool IsValid(ITargetable target)
        {
            if (!_candidateResolver.TryResolve(target, out var candidate))
                return false;

            if (!_eligibilityService.IsEligible(candidate))
                return false;

            return _lineOfSightChecker.HasLineOfSight(candidate);
        }
    }
}
