using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Pickup
{
    public interface IWorldPickup
    {
        bool IsCollectable { get; }

        PickupDefinition Definition { get; }

        UniTask SetCollectedAsync(CancellationToken token);
    }
}