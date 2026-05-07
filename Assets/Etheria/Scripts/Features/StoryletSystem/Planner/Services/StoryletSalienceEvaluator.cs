using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletSalienceEvaluator : IStoryletSalienceEvaluator
    {
        public StoryletSalienceEvaluation Evaluate(
            StoryletDefinition definition,
            IReadOnlyList<RoleAssignment> assignment,
            StoryletPlannerMemory memory)
        {
            var bonus = definition.SaliencePolicy.BaseWeight + definition.SaliencePolicy.UnlockBonus;
            var antiRepetitionPenalty = 0f;

            if (memory.ExecutionStepByStoryletId.TryGetValue(definition.Id, out var lastStoryletStep))
            {
                antiRepetitionPenalty -= Math.Max(
                    0f,
                    definition.SaliencePolicy.RecentRepeatPenalty - (memory.CurrentStep - lastStoryletStep));
            }

            foreach (var roleAssignment in assignment)
            {
                var key = StoryletRepeatabilityService.BuildRolePairingKey(definition.Id, roleAssignment);

                if (memory.LastStepByRolePairingKey.TryGetValue(key, out var lastPairingStep))
                {
                    antiRepetitionPenalty -= Math.Max(
                        0f,
                        definition.SaliencePolicy.RepeatedPairPenalty - (memory.CurrentStep - lastPairingStep) * 0.5f);
                }
            }

            return new StoryletSalienceEvaluation(bonus, antiRepetitionPenalty);
        }
    }
}
