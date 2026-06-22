using System.Collections.Generic;
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
    }
}