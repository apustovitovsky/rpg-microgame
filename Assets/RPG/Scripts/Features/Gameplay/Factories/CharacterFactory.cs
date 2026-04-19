using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public sealed class CharacterFactory : ICharacterFactory
    {
        private readonly LifetimeScope _parentScope;

        public CharacterFactory(LifetimeScope parentScope)
        {
            _parentScope = parentScope;
        }

        public LifetimeScope Create(LifetimeScope characterPrefab, Vector3 position)
        {
            using (LifetimeScope.EnqueueParent(_parentScope))
            {
                var instance = Object.Instantiate(characterPrefab, position, Quaternion.identity);
                instance.name = characterPrefab.name;
                return instance;
            }
        }
    }
}
