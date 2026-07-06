using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Pickup
{
    public interface IPickupCollector
    {
        bool CanReceive(PickupContext context);

        UniTask ReceiveAsync(
            PickupContext context,
            CancellationToken token);
    }
}