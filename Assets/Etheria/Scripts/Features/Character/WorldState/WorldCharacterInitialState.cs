using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Etheria.Features.Character
{
    [Serializable]
    public sealed class WorldCharacterInitialState
    {
        [field: SerializeField]
        public CharacterDefinitionSO Character { get; private set; }

        [field: SerializeField]
        [field: FormerlySerializedAs("<SpawnPointId>k__BackingField")]
        public string LocationId { get; private set; }

        [field: SerializeField]
        public bool IsAlive { get; private set; } = true;
    }
}
