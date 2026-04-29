using Etheria.Game.Player;

namespace Etheria.Features.Targeting
{
    public interface ITargetCandidateFilter
    {
        bool IsAllowed(TargetCandidate candidate);
    }

    public sealed class ViewConeTargetCandidateFilter : ITargetCandidateFilter
    {
        private readonly TargetingSettingsSO _settings;
        private readonly IPlayerAvatarProvider _playerAvatarProvider;

        public ViewConeTargetCandidateFilter(
            TargetingSettingsSO settings,
            IPlayerAvatarProvider playerAvatarProvider)
        {
            _settings = settings;
            _playerAvatarProvider = playerAvatarProvider;
        }

        public bool IsAllowed(TargetCandidate candidate)
        {
            if (candidate.Targetable == null)
                return false;

            if (!candidate.Targetable.IsTargetable)
                return false;

            var currentAvatar = _playerAvatarProvider.Current;
            if (currentAvatar.HasValue &&
                ReferenceEquals(candidate.Targetable, currentAvatar.Value.Targetable))
            {
                return false;
            }

            if (candidate.Distance > _settings.MaxDistance)
                return false;

            if (candidate.Angle > _settings.MaxAngle)
                return false;

            return true;
        }
    }
}
