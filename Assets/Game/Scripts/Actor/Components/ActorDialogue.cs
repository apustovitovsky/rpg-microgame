using System.Threading;
using Cysharp.Threading.Tasks;
using Game.CommandSystem;
using UnityEngine;

namespace Game.Actor
{
    public sealed class ActorDialogue :
        MonoBehaviour,
        IActorDialogueHandler
    {
        [SerializeField] private MonoBehaviour _handler;

        public UniTask<CommandStatus> StartDialogueAsync(
            string targetActorId,
            CancellationToken cancellationToken)
        {
            if (_handler is not IActorDialogueHandler handler)
            {
                return UniTask.FromResult(
                    CommandStatus.HandlerNotFound);
            }

            return handler.StartDialogueAsync(
                targetActorId,
                cancellationToken);
        }

        private void OnValidate()
        {
            if (_handler != null &&
                _handler is not IActorDialogueHandler)
            {
                Debug.LogError(
                    $"{nameof(ActorDialogue)} handler must implement {nameof(IActorDialogueHandler)}.",
                    this);

                _handler = null;
            }
        }
    }
}