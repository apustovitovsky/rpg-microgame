using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;
using UnityEngine;

namespace Game.Pickup
{
    [DisallowMultipleComponent]
    public sealed class WorldPickup :
        MonoBehaviour,
        IWorldPickup,
        IDisplayInfo
    {
        private bool _isCollected;

        public WorldId WorldId { get; private set; }

        public PickupDefinition Definition { get; private set; }

        public bool IsCollectable =>
            !_isCollected &&
            isActiveAndEnabled &&
            gameObject.activeInHierarchy &&
            !WorldId.IsEmpty &&
            Definition != null;

        public string DisplayName =>
            Definition != null && !string.IsNullOrWhiteSpace(Definition.DisplayName)
                ? Definition.DisplayName
                : WorldId.ToString();

        public void Initialize(
            WorldId worldId,
            PickupDefinition definition)
        {
            WorldId = worldId;
            Definition = definition;
            _isCollected = false;
        }

        public UniTask SetCollectedAsync(CancellationToken token)
        {
            _isCollected = true;
            return UniTask.CompletedTask;
        }
    }
}