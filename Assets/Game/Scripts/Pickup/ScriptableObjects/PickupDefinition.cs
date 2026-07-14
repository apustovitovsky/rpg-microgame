using System;
using Game.Core;
using UnityEngine;

namespace Game.Pickup
{
    [CreateAssetMenu(
        fileName = "PickupDefinition",
        menuName = "Game/Pickup/Pickup Definition")]
    public sealed class PickupDefinition :
        AssetDefinition<PickupInstance, PickupFragment>
    {
        [field: SerializeField]
        public GameObject Prefab { get; private set; }

        public override PickupInstance CreateInstance(
            Guid? instanceId = null)
        {
            return new PickupInstance(
                instanceId ?? Guid.NewGuid(),
                this);
        }
    }
}