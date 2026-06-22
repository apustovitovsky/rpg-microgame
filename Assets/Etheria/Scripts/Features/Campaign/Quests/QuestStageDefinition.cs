using System;
using UnityEngine;

namespace Etheria.Features.Campaign
{
    [Serializable]
    public sealed class QuestStageDefinition
    {
        [field: SerializeField]
        public int Value { get; private set; }
    }
}