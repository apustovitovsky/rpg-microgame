using System;
using Etheria.Game.World;
using Etheria.Navigation;
using UnityEngine;

namespace Etheria.Features.Campaign
{
    [CreateAssetMenu(menuName = "Etheria/Campaign/Quest")]
    public sealed class QuestDefinitionSO : ScriptableObject
    {
        [field: SerializeField]
        public string Id { get; private set; }

        [field: SerializeField]
        public QuestStageDefinition[] Stages { get; private set; }

        [field: SerializeField]
        public QuestTravelInstruction[] TravelInstructions { get; private set; }

        private void OnValidate()
        {
            Id = Id?.Trim();
        }

        public bool ContainsStage(int stage)
        {
            if (Stages == null)
                return false;

            foreach (var definition in Stages)
            {
                if (definition != null &&
                    definition.Value == stage)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetTravelInstruction(
            string instructionId,
            out QuestTravelInstruction instruction)
        {
            instructionId = instructionId?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(instructionId) &&
                TravelInstructions != null)
            {
                foreach (var candidate in TravelInstructions)
                {
                    if (candidate != null &&
                        string.Equals(
                            candidate.Id?.Trim(),
                            instructionId,
                            StringComparison.Ordinal))
                    {
                        instruction = candidate;
                        return true;
                    }
                }
            }

            instruction = null;
            return false;
        }
    }

    [Serializable]
    public sealed class QuestTravelInstruction
    {
        [field: SerializeField]
        public string Id { get; private set; }

        [field: SerializeField]
        public string NpcId { get; private set; }

        [field: SerializeField]
        public string LocationId { get; private set; }

        [field: SerializeField]
        public string AnchorKey { get; private set; } = NavigationAnchorKeys.Default;

        [field: SerializeField]
        public NavigationQueryFilterSO Filter { get; private set; }

        public NavigationQueryFilter QueryFilter =>
            Filter != null
                ? Filter.ToFilter()
                : NavigationQueryFilter.Any;
    }
}