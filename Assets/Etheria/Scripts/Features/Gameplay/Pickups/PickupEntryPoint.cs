using System;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.Gameplay
{
    public class PickupEntryPoint : IStartable
    {
        private const int PickupCount = 10;
        private const float SpawnRadius = 15f;

        private readonly PickupDefinitionSO _pickupDefinition;
        private readonly PickupPool _pickupPool;

        public PickupEntryPoint(PickupPool pickupPool, PickupDefinitionSO pickupDefinition)
        {
            _pickupPool = pickupPool;
            _pickupDefinition = pickupDefinition;
        }

        public void Start()
        {
            // for (int i = 0; i < PickupCount; i++)
            // {
            //     var offset2D = UnityEngine.Random.insideUnitCircle * SpawnRadius;
            //     var position = new Vector3(offset2D.x, 0f, offset2D.y);
            //     _pickupPool.Get(_pickupDefinition, position, Quaternion.identity);
            // }
        }
    }
}

