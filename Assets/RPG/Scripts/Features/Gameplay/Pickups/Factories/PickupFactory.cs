using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public sealed class PickupFactory : IPickupFactory
    {
        readonly IObjectResolver _container;

        public PickupFactory(IObjectResolver container)
        {
            _container = container;
        }

        public void Create(Pickup prefab, Vector3 position)
        {
            _container.Instantiate(prefab, position, Quaternion.identity);
        }
    }
}