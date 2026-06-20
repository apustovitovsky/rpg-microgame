using UnityEngine;

namespace Etheria.Game.Character
{
    public interface IPlayerCharacterProvider
    {
        Transform Current { get; }
        void Set(Transform character);
    }
}