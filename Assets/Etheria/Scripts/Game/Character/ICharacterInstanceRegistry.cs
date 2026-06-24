using UnityEngine;

namespace Etheria.Game.Character
{
    public interface ICharacterInstanceRegistry
    {
        bool TryGetInstance(
            string characterId,
            out GameObject instance);
    }
}