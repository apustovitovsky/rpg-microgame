using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Pickup
{
    public interface ICollectable
    {
        WorldId WorldId { get; }

        bool CanCollect(WorldId collectorId);

        UniTask<CollectResult> CollectAsync(
            WorldId collectorId,
            CancellationToken token);
    }
}