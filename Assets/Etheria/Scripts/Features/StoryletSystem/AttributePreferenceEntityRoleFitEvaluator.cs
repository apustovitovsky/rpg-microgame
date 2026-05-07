using System;
using Unity.Mathematics;

namespace Etheria.Features.StoryletSystem
{
    public sealed class AttributePreferenceEntityRoleFitEvaluator : IEntityRoleFitEvaluator
    {
        public float Evaluate(Entity entity, Role role)
        {
            if (role.AttributePreferences.Length == 0)
            {
                return 1f;
            }

            var weightedScoreSum = 0f;
            var absoluteWeightSum = 0f;

            foreach (var attributePreference in role.AttributePreferences)
            {
                var value = entity.Attributes.GetOrDefault(attributePreference.AttributeId);
                var factorScore = math.smoothstep(
                    attributePreference.Start,
                    attributePreference.End,
                    value);
                weightedScoreSum += factorScore * attributePreference.Weight;
                absoluteWeightSum += MathF.Abs(attributePreference.Weight);
            }

            if (absoluteWeightSum <= 0f)
            {
                return 1f;
            }

            var normalizedScore = weightedScoreSum / absoluteWeightSum;
            return 1f + normalizedScore;
        }
    }
}
