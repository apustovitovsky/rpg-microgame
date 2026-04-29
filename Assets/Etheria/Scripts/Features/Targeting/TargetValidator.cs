using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public sealed class TargetValidator : ITargetValidator
    {
        private readonly ITargetCandidateResolver _candidateResolver;
        private readonly ITargetLineOfSightChecker _lineOfSightChecker;
        private readonly TargetingSettingsSO _settings;

        public TargetValidator(
            ITargetCandidateResolver candidateResolver,
            ITargetLineOfSightChecker lineOfSightChecker,
            TargetingSettingsSO settings)
        {
            _candidateResolver = candidateResolver;
            _lineOfSightChecker = lineOfSightChecker;
            _settings = settings;
        }

        public bool IsValid(ITargetable target)
        {
            if (!_candidateResolver.TryResolve(target, out var candidate))
                return false;

            if (candidate.Distance > _settings.MaxDistance)
                return false;

            if (candidate.Angle > _settings.MaxAngle)
                return false;

            return _lineOfSightChecker.HasLineOfSight(candidate);
        }
    }
}
