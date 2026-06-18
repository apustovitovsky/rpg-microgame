namespace Etheria.Features.Targeting
{
    public sealed class TargetCandidateValidityFilter : ITargetCandidateValidityFilter
    {
        private readonly ITargetValidator _targetValidator;

        public TargetCandidateValidityFilter(ITargetValidator targetValidator)
        {
            _targetValidator = targetValidator;
        }

        public int FilterInPlace(TargetCandidate[] candidates, int count)
        {
            if (candidates == null || count <= 0)
                return 0;

            var validCount = 0;
            for (var i = 0; i < count; i++)
            {
                var candidate = candidates[i];

                if (!_targetValidator.IsValid(candidate.Targetable))
                    continue;

                candidates[validCount++] = candidate;
            }

            return validCount;
        }
    }
}
