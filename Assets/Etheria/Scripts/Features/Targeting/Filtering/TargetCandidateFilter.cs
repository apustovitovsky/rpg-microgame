using Etheria.Game.Actor;
using Etheria.Game.Player;

namespace Etheria.Features.Targeting
{
    public interface ITargetCandidateFilter
    {
        bool IsAllowed(TargetCandidate candidate);
    }

    public sealed class TargetCandidateFilter : ITargetCandidateFilter
    {
        private readonly ITargetEligibilityService _eligibilityService;
        private readonly IPlayerAvatarProvider _playerAvatarProvider;

        public TargetCandidateFilter(
            ITargetEligibilityService eligibilityService,
            IPlayerAvatarProvider playerAvatarProvider)
        {
            _eligibilityService = eligibilityService;
            _playerAvatarProvider = playerAvatarProvider;
        }

        public bool IsAllowed(TargetCandidate candidate)
        {
            if (!_eligibilityService.IsEligible(candidate))
                return false;

            var currentAvatar = _playerAvatarProvider.Current;
            if (currentAvatar != null &&
                candidate.Targetable.Id == currentAvatar.Info.Id)
            {
                return false;
            }

            return true;
        }
    }
}
