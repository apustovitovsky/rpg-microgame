using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Pickup
{
    public interface IPickup
    {
        WorldId WorldId { get; }

        PickupDefinition Definition { get; }

        bool IsCollectable { get; }

        bool IsCollected { get; }

        UniTask SetCollectedAsync(CancellationToken token);
    }
}