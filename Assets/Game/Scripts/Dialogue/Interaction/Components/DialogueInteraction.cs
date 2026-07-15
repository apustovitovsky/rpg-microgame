using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Interaction;
using UnityEngine;

namespace Game.Dialogue.Interaction
{
    public sealed class DialogueInteraction :
        IInteractionTarget
    {
        private readonly DialogueInteractionSettings _settings;
        private readonly IDialogueParticipant _dialogue;

        public DialogueInteraction(
            DialogueInteractionSettings settings,
            IDialogueParticipant dialogue)
        {
            _settings = settings;

            _dialogue = dialogue
                ?? throw new ArgumentNullException(nameof(dialogue));
        }

        public Vector3 InteractionPoint =>
            _settings.InteractionPoint.position;

        public float MaxRange =>
            _settings.MaxRange;

        public bool CanInteract(
            InteractionContext context)
        {
            return _dialogue.Evaluate(
                       context.InteractorInstanceId) ==
                   DialogueEvaluationStatus.Available;
        }

        public async UniTask<InteractionResult> InteractAsync(
            InteractionContext context,
            CancellationToken token)
        {
            if (!CanInteract(context))
            {
                return InteractionResult.Rejected;
            }

            var result = await _dialogue.StartDialogueAsync(
                context.InteractorInstanceId,
                token);

            return result.Status switch
            {
                DialogueStartStatus.Started =>
                    InteractionResult.Completed,

                DialogueStartStatus.Busy =>
                    InteractionResult.Busy,

                _ => InteractionResult.Rejected
            };
        }
    }
}