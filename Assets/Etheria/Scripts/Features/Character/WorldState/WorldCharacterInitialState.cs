using System;
using Etheria.Game.World;
using UnityEngine;

namespace Etheria.Features.Character
{
    [Serializable]
    public sealed class WorldCharacterInitialState
    {
        [field: SerializeField]
        public CharacterDefinitionSO Character { get; private set; }

        [field: SerializeField]
        public string LocationId { get; private set; }

        [field: SerializeField]
        public string AnchorKey { get; private set; } = NavigationAnchorKeys.Default;

        [field: SerializeField]
        public bool IsAlive { get; private set; } = true;

        [field: SerializeField]
        public bool IsPresent { get; private set; } = true;
    }
}