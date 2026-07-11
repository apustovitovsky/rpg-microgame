using Game.Core;
using Game.Item;
using UnityEngine;

namespace Game.Pickup
{
    [CreateAssetMenu(
        fileName = "PickupDefinition",
        menuName = "Game/Pickup/Pickup Definition")]
    public sealed class PickupDefinition : Definition
    {
        [field: SerializeField]
        public GameObject Prefab { get; private set; }

        [field: SerializeField]
        public ItemDefinition Item { get; private set; }

        [field: SerializeField, Min(1)]
        public int Amount { get; private set; } = 1;

        protected override void OnValidate()
        {
            base.OnValidate();

            if (Amount < 1)
                Amount = 1;
        }
    }
}