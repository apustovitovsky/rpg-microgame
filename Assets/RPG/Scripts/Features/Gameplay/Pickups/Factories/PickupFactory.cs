using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public sealed class PickupFactory : IPickupFactory
    {
        private readonly IObjectResolver _container;
        private readonly Pickup _prefab;

        public PickupFactory(IObjectResolver container, Pickup prefab)
        {
            _container = container;
            _prefab = prefab;
        }

        public Pickup Create(PickupDefinitionSO definition, Vector3 position)
        {

            var pickup = _container.Instantiate(_prefab, position, Quaternion.identity);
            pickup.SetInstance(new PickupInstance(definition));

            return pickup;
        }
    }
}
