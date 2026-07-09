// using System.Threading;
// using Cysharp.Threading.Tasks;
// using Game.Pickup;
// using UnityEngine;

// namespace Game.Actor
// {
//     public sealed class DebugActorViewPickupEffectHandler :
//         PickupEffectHandler<DebugActorViewPickupEffect>
//     {
//         private readonly IActorView _anchors;

//         public DebugActorViewPickupEffectHandler(IActorView anchors)
//         {
//             _anchors = anchors;
//         }

//         protected override bool CanApply(
//             DebugActorViewPickupEffect effect,
//             IWorldPickup pickup)
//         {
//             return effect != null &&
//                    pickup != null &&
//                    _anchors != null;
//         }

//         protected override UniTask ApplyAsync(
//             DebugActorViewPickupEffect effect,
//             IWorldPickup pickup,
//             CancellationToken token)
//         {
//             Debug.Log(
//                 $"{effect.Message}. Actor: '{_anchors.Root.name}'. Pickup: '{pickup.Definition?.name}'.");

//             return UniTask.CompletedTask;
//         }
//     }
// }