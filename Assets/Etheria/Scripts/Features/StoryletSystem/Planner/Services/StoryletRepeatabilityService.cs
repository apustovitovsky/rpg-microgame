using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletRepeatabilityService : IStoryletRepeatabilityService
    {
        public bool IsBlocked(
            StoryletDefinition definition,
            StoryletWorldState worldState,
            StoryletPlannerMemory memory,
            out StoryletRejectionReason rejectionReason)
        {
            rejectionReason = null;
            var policy = definition.RepeatabilityPolicy;

            switch (policy.Mode)
            {
                case StoryletRepeatabilityMode.OnceEver:
                case StoryletRepeatabilityMode.OncePerRun:
                    if (memory.ExecutionCountByStoryletId.ContainsKey(definition.Id))
                    {
                        rejectionReason = new StoryletRejectionReason(
                            "repeatability_lock",
                            $"'{definition.Key}' is locked by {policy.Mode} policy.");
                        return true;
                    }

                    return false;
                case StoryletRepeatabilityMode.RepeatableWithCooldown:
                    if (memory.ExecutionStepByStoryletId.TryGetValue(definition.Id, out var lastStep)
                        && memory.CurrentStep - lastStep <= policy.CooldownSteps)
                    {
                        rejectionReason = new StoryletRejectionReason(
                            "repeatability_lock",
                            $"'{definition.Key}' is cooling down for {policy.CooldownSteps} steps.");
                        return true;
                    }

                    return false;
                case StoryletRepeatabilityMode.RepeatableUntilStateChanges:
                    if (memory.LastWorldFingerprintByStoryletId.TryGetValue(definition.Id, out var fingerprint)
                        && string.Equals(fingerprint, worldState.GetFingerprint(), StringComparison.Ordinal))
                    {
                        rejectionReason = new StoryletRejectionReason(
                            "repeatability_lock",
                            $"'{definition.Key}' already fired on the same world state.");
                        return true;
                    }

                    return false;
                default:
                    return false;
            }
        }

        public StoryletPlannerMemory Advance(
            StoryletPlannerMemory memory,
            StoryletInstantiationCandidate candidate,
            StoryletWorldState nextWorldState)
        {
            var executionSteps = new Dictionary<StoryletId, int>(memory.ExecutionStepByStoryletId);
            var executionCounts = new Dictionary<StoryletId, int>(memory.ExecutionCountByStoryletId);
            var pairingHistory = new Dictionary<string, int>(memory.LastStepByRolePairingKey, StringComparer.Ordinal);
            var lastFingerprints = new Dictionary<StoryletId, string>(memory.LastWorldFingerprintByStoryletId);
            var nextStep = memory.CurrentStep + 1;

            executionSteps[candidate.Definition.Id] = nextStep;
            executionCounts[candidate.Definition.Id] = executionCounts.TryGetValue(candidate.Definition.Id, out var count)
                ? count + 1
                : 1;
            lastFingerprints[candidate.Definition.Id] = nextWorldState.GetFingerprint();

            foreach (var roleAssignment in candidate.Assignment)
            {
                pairingHistory[BuildRolePairingKey(candidate.Definition.Id, roleAssignment)] = nextStep;
            }

            return new StoryletPlannerMemory(
                nextStep,
                executionSteps,
                executionCounts,
                pairingHistory,
                lastFingerprints);
        }

        public static string BuildRolePairingKey(StoryletId storyletId, RoleAssignment assignment)
        {
            return $"{storyletId.Value}:{assignment.Role.Id.Value}:{assignment.Entity.Id.Value}";
        }
    }
}
