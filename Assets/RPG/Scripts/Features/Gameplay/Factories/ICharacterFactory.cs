using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public interface ICharacterFactory
    {
        LifetimeScope Create(LifetimeScope characterPrefab, Vector3 position);
    }
}
