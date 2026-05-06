namespace Etheria.Features.Targeting
{
    public sealed class TargetEligibilityService : ITargetEligibilityService
    {
        private readonly TargetingSettingsSO _settings;

        public TargetEligibilityService(TargetingSettingsSO settings)
        {
            _settings = settings;
        }

        public bool IsEligible(TargetCandidate candidate)
        {
            if (candidate.Targetable == null)
                return false;

            if (!candidate.Targetable.IsTargetable)
                return false;

            if (candidate.Distance > _settings.MaxDistance)
                return false;

            if (candidate.Angle > _settings.MaxAngle)
                return false;

            return true;
        }
    }
}
