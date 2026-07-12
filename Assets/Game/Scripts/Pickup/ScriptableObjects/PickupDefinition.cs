using System;
using Game.Item;
using Game.World;
using UnityEngine;

namespace Game.Pickup
{
    [CreateAssetMenu(
        fileName = "PickupDefinition",
        menuName = "Game/Pickup/Pickup Definition")]
    public sealed class PickupDefinition :
        WorldDefinition<PickupInstance>
    {
        [field: SerializeField]
        public ItemDefinition Item { get; private set; }

        [field: SerializeField, Min(1)]
        public int Amount { get; private set; } = 1;

        public override PickupInstance CreateInstance(
            Guid? instanceId = null)
        {
            return new PickupInstance(
                instanceId ?? Guid.NewGuid(),
                this);
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (Amount < 1)
                Amount = 1;
        }
    }
}