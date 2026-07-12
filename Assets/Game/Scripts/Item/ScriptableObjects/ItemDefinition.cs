using System;
using Game.Core;
using UnityEngine;

namespace Game.Item
{
    [CreateAssetMenu(
        fileName = "ItemDefinition",
        menuName = "Game/Item/Item Definition")]
    public sealed class ItemDefinition :
        AssetDefinition<ItemInstance>
    {
        [field: SerializeField, Min(1)]
        public int MaxStackSize { get; private set; } = 1;

        public override ItemInstance CreateInstance(
            Guid? instanceId = null)
        {
            return new ItemInstance(
                instanceId ?? Guid.NewGuid(),
                this);
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (MaxStackSize < 1)
                MaxStackSize = 1;
        }
    }
}