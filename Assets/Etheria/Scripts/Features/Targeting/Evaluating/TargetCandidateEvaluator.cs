using Etheria.Game.Targeting;
using UnityEngine;

namespace Etheria.Features.Targeting
{
    public sealed class TargetCandidateEvaluator : ITargetCandidateEvaluator
    {
        private readonly TargetingSettingsSO _settings;

        public TargetCandidateEvaluator(TargetingSettingsSO settings)
        {
            _settings = settings;
        }

        public float Evaluate(TargetCandidate candidate, ITargetable currentTarget)
        {
            var angle01 = Mathf.Clamp01(candidate.Angle / _settings.MaxAngle);
            var distance01 = Mathf.Clamp01(candidate.Distance / _settings.MaxDistance);

            var score = 0f;

            score += (1f - angle01) * _settings.AngleWeight;
            score += (1f - distance01) * _settings.DistanceWeight;

            if (ReferenceEquals(candidate.Targetable, currentTarget))
                score += _settings.CurrentTargetBonus;

            return score;
        }
    }
}