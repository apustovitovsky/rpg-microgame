// using System.Threading;
// using Cysharp.Threading.Tasks;
// using Game.CommandSystem;
// using UnityEngine;

// namespace Game.Actor
// {
//     public sealed class ActorTravel :
//         MonoBehaviour,
//         IActorTravelHandler
//     {
//         [SerializeField] private MonoBehaviour _handler;

//         public UniTask<CommandStatus> MoveToLocationAsync(
//             string locationId,
//             string anchorKey,
//             CancellationToken cancellationToken)
//         {
//             if (_handler is not IActorTravelHandler handler)
//             {
//                 return UniTask.FromResult(CommandStatus.HandlerNotFound);
//             }

//             return handler.MoveToLocationAsync(
//                 locationId,
//                 anchorKey,
//                 cancellationToken);
//         }

//         private void OnValidate()
//         {
//             if (_handler != null &&
//                 _handler is not IActorTravelHandler)
//             {
//                 Debug.LogError(
//                     $"{nameof(ActorTravel)} handler must implement {nameof(IActorTravelHandler)}.",
//                     this);

//                 _handler = null;
//             }
//         }
//     }
// }