using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;
using UnityEngine;

namespace Game.Pickup
{
    [DisallowMultipleComponent]
    public sealed class PickupComponent :
        MonoBehaviour,
        IPickup
    {
        public WorldId WorldId { get; private set; }

        public PickupDefinition Definition { get; private set; }

        public bool IsCollected { get; private set; }

        public bool IsCollectable =>
            !IsCollected &&
            isActiveAndEnabled &&
            gameObject.activeInHierarchy &&
            !WorldId.IsEmpty &&
            Definition != null;

        public void Initialize(
            WorldId worldId,
            PickupDefinition definition)
        {
            WorldId = worldId;
            Definition = definition;
            IsCollected = false;
        }

        public UniTask SetCollectedAsync(CancellationToken token)
        {
            IsCollected = true;
            return UniTask.CompletedTask;
        }
    }
}