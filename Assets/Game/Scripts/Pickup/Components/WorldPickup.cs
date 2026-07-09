using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Pickup
{
    public sealed class WorldPickup : IWorldPickup
    {
        private bool _isCollected;

        public WorldPickup(
            WorldInfo info,
            PickupDefinition definition)
        {
            Info = info;
            Definition = definition;
        }

        public WorldInfo Info { get; }

        public WorldId WorldId => Info.WorldId;

        public PickupDefinition Definition { get; }

        public bool IsCollectable =>
            !_isCollected &&
            !WorldId.IsEmpty &&
            Definition != null;

        public UniTask SetCollectedAsync(CancellationToken token)
        {
            _isCollected = true;
            return UniTask.CompletedTask;
        }
    }
}