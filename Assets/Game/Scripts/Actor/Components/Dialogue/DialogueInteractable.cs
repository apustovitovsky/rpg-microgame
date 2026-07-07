using System.Threading;
using Cysharp.Threading.Tasks;
using Game.CommandSystem;
using Game.Interaction;
using Game.World;
using UnityEngine;

namespace Game.Actor
{
    public sealed class DialogueInteractable :
        MonoBehaviour,
        IInteractable,
        IActorDialogueEndpoint
    {

        [field: SerializeField] public float MaxRange { get; private set; } = 5f;

        public bool CanInteract(InteractionContext context)
        {
            return context.Interactor != null &&
                   context.Target != null &&
                   context.Interactor.WorldId != context.Target.WorldId;
        }

        public async UniTask InteractAsync(
            InteractionContext context,
            CancellationToken token)
        {
            await StartDialogueAsync(
                context.Interactor.WorldId.ToString(),
                token);
        }

        public UniTask<CommandStatus> StartDialogueAsync(
            WorldId interactorWorldId,
            CancellationToken cancellationToken)
        {
            Debug.Log(
                $"Dialogue started by actor '{interactorWorldId}'.",
                this);

            return UniTask.FromResult(CommandStatus.Succeeded);
        }
    }
}