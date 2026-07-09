using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;
using UnityEngine;

namespace Game.Pickup
{
    public sealed class DebugPickupEffectHandler :
        PickupEffectHandler<DebugPickupEffect>
    {
        protected override bool CanApply(
            WorldId collectorId,
            DebugPickupEffect effect,
            IPickup pickup)
        {
            return !collectorId.IsEmpty &&
                   effect != null &&
                   pickup != null;
        }

        protected override UniTask ApplyAsync(
            WorldId collectorId,
            DebugPickupEffect effect,
            IPickup pickup,
            CancellationToken token)
        {
            Debug.Log(
                $"{effect.Message}. Collector: '{collectorId}'. Pickup: '{pickup.WorldId}'.");

            return UniTask.CompletedTask;
        }
    }
}