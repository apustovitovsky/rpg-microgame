
using Etheria.Game.Targeting;
using UnityEngine;

namespace Etheria.Features.Targeting
{
    public interface ITargetCandidateFilter
    {
        bool IsAllowed(TargetCandidate candidate, ITargetable ignoredTarget);
    }

    public sealed class ViewConeTargetCandidateFilter : ITargetCandidateFilter
    {
        private readonly TargetingSettingsSO _settings;

        public ViewConeTargetCandidateFilter(TargetingSettingsSO settings)
        {
            _settings = settings;
        }

        public bool IsAllowed(TargetCandidate candidate, ITargetable ignoredTarget)
        {
            if (candidate.Targetable == null)
                return false;

            if (!candidate.Targetable.IsTargetable)
                return false;

            if (ReferenceEquals(candidate.Targetable, ignoredTarget))
                return false;

            if (candidate.Distance > _settings.MaxDistance)
                return false;

            if (candidate.Angle > _settings.MaxAngle)
                return false;

            return true;
        }
    }
}