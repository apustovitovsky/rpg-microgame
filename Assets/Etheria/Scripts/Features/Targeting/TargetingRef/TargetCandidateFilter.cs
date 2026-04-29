using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public interface ITargetCandidateFilter
    {
        bool IsAllowed(TargetCandidate candidate);
    }

    public sealed class ViewConeTargetCandidateFilter : ITargetCandidateFilter
    {
        private readonly TargetingSettingsSO _settings;
        private readonly IControlledTargetProvider _controlledTargetProvider;

        public ViewConeTargetCandidateFilter(
            TargetingSettingsSO settings,
            IControlledTargetProvider controlledTargetProvider)
        {
            _settings = settings;
            _controlledTargetProvider = controlledTargetProvider;
        }

        public bool IsAllowed(TargetCandidate candidate)
        {
            if (candidate.Targetable == null)
                return false;

            if (!candidate.Targetable.IsTargetable)
                return false;

            if (ReferenceEquals(candidate.Targetable, _controlledTargetProvider.ControlledTarget))
                return false;

            if (candidate.Distance > _settings.MaxDistance)
                return false;

            if (candidate.Angle > _settings.MaxAngle)
                return false;

            return true;
        }
    }
}
