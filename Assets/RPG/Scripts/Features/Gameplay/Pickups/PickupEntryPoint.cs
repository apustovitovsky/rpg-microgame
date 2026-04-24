using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public class PickupEntryPoint : IStartable
    {
        private readonly PickupDefinitionSO _pickupDefinition;
        private readonly IPickupFactory _pickupFactory;

        public PickupEntryPoint(IPickupFactory pickupFactory, PickupDefinitionSO pickupDefinition)
        {
            _pickupFactory = pickupFactory;
            _pickupDefinition = pickupDefinition;
        }

        public void Start()
        {
            Debug.Log($"Spawning {_pickupDefinition.DisplayName}");
            _pickupFactory.Create(_pickupDefinition, Vector3.zero);
        }
    }
}