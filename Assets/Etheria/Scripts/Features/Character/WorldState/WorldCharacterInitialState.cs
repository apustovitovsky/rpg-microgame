using System;
using UnityEngine;

namespace Etheria.Features.Character
{
    [Serializable]
    public sealed class WorldCharacterInitialState
    {
        [field: SerializeField]
        public CharacterDefinitionSO Character { get; private set; }

        [field: SerializeField]
        public string SpawnPointId { get; private set; }

        [field: SerializeField]
        public bool IsAlive { get; private set; } = true;
    }
}