using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Pickup
{
    public interface IPickup
    {
        bool CanCollect(PickupContext context);

        UniTask CollectAsync(
            PickupContext context,
            CancellationToken token);
    }

    public readonly struct PickupContext
    {
        public PickupContext(IPickup pickup)
        {
            Pickup = pickup;
        }

        public IPickup Pickup { get; }
    }
}