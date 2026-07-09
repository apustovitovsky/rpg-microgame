using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Pickup
{
    public interface IWorldPickup
    {
        WorldInfo Info { get; }

        WorldId WorldId { get; }

        PickupDefinition Definition { get; }

        bool IsCollectable { get; }

        UniTask SetCollectedAsync(CancellationToken token);
    }
}