using System;
using System.Collections.Generic;
using System.Linq;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletPlannerMemory
    {
        public StoryletPlannerMemory(
            int currentStep,
            IReadOnlyDictionary<StoryletId, int> executionStepByStoryletId,
            IReadOnlyDictionary<StoryletId, int> executionCountByStoryletId,
            IReadOnlyDictionary<string, int> lastStepByRolePairingKey,
            IReadOnlyDictionary<StoryletId, string> lastFingerprintByStoryletId)
        {
            CurrentStep = currentStep;
            ExecutionStepByStoryletId = executionStepByStoryletId
                ?? new Dictionary<StoryletId, int>();
            ExecutionCountByStoryletId = executionCountByStoryletId
                ?? new Dictionary<StoryletId, int>();
            LastStepByRolePairingKey = lastStepByRolePairingKey
                ?? new Dictionary<string, int>(StringComparer.Ordinal);
            LastWorldFingerprintByStoryletId = lastFingerprintByStoryletId
                ?? new Dictionary<StoryletId, string>();
        }

        public int CurrentStep { get; }
        public IReadOnlyDictionary<StoryletId, int> ExecutionStepByStoryletId { get; }
        public IReadOnlyDictionary<StoryletId, int> ExecutionCountByStoryletId { get; }
        public IReadOnlyDictionary<string, int> LastStepByRolePairingKey { get; }
        public IReadOnlyDictionary<StoryletId, string> LastWorldFingerprintByStoryletId { get; }

        public static StoryletPlannerMemory Empty { get; } = new(
            0,
            new Dictionary<StoryletId, int>(),
            new Dictionary<StoryletId, int>(),
            new Dictionary<string, int>(StringComparer.Ordinal),
            new Dictionary<StoryletId, string>());

        public string GetFingerprint()
        {
            var storyletHistory = string.Join(
                ",",
                ExecutionStepByStoryletId.OrderBy(pair => pair.Key.Value)
                    .Select(pair => $"{pair.Key.Value}:{pair.Value}"));
            var pairingHistory = string.Join(
                ",",
                LastStepByRolePairingKey.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => $"{pair.Key}:{pair.Value}"));

            return $"{CurrentStep}#{storyletHistory}#{pairingHistory}";
        }
    }
}
