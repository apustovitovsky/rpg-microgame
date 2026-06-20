using Etheria.Game.Character;
using UnityEngine;

namespace Etheria.Features.Character
{
    public sealed class PlayerCharacterProvider :
        IPlayerCharacterProvider
    {
        public Transform Current { get; private set; }

        public void Set(Transform character)
        {
            Current = character;
        }
    }
}