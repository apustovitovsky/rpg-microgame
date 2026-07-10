using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Pickup
{
    public interface ICollectable
    {
        Guid InstanceId { get; }

        bool CanCollect(Guid collectorInstanceId);

        UniTask<CollectResult> CollectAsync(
            Guid collectorInstanceId,
            CancellationToken token);
    }
}