using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Pickup
{
    public interface IPickup
    {
        bool IsCollectable { get; }

        PickupDefinition Definition { get; }

        UniTask MarkCollectedAsync(CancellationToken token);
    }
}