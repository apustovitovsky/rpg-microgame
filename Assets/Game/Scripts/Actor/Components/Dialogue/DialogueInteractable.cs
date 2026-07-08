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
        IActorDialogue
    {
        [SerializeField] private Transform _interactionPoint;

        [field: SerializeField]
        public float MaxRange { get; private set; } = 5f;

        public Vector3 InteractionPosition =>
            _interactionPoint != null
                ? _interactionPoint.position
                : transform.position;

        public bool CanInteract(InteractionContext context)
        {
            return !context.InteractorWorldId.IsEmpty &&
                   !context.TargetWorldId.IsEmpty &&
                   context.InteractorWorldId != context.TargetWorldId;
        }

        public async UniTask InteractAsync(
            InteractionContext context,
            CancellationToken token)
        {
            await StartDialogueAsync(
                context.InteractorWorldId,
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