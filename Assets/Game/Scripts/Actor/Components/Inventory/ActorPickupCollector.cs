using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Pickup;

namespace Game.Actor
{
    public sealed class ActorPickupCollector : IPickupCollector
    {
        public bool CanReceive(PickupContext context)
        {
            return true;
        }

        public UniTask ReceiveAsync(
            PickupContext context,
            CancellationToken token)
        {
            return UniTask.CompletedTask;
        }
    }
}