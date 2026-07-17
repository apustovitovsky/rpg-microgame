using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using UnityEngine;

namespace Game.Dialogue.Actor
{
    public sealed class DialogueParticipant :
        IDialogueParticipant
    {
        private readonly Guid _speakerInstanceId;
        private readonly DialogueEntry _entry;
        private readonly IDialogueCoordinator _coordinator;

        public DialogueParticipant(
            IInstanceIdentity identity,
            IFragmentProvider fragments,
            IDialogueCoordinator coordinator)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            if (identity.InstanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Dialogue speaker instance id is required.",
                    nameof(identity));
            }

            if (fragments == null)
            {
                throw new ArgumentNullException(nameof(fragments));
            }

            if (!fragments.TryGetFragment(
                    out DialogueFragment fragment) ||
                fragment.Definition == null ||
                !fragment.Definition.Entry.IsValid)
            {
                throw new InvalidOperationException(
                    $"{nameof(DialogueFragment)} with a valid " +
                    $"{nameof(DialogueDefinition)} is required.");
            }

            _speakerInstanceId = identity.InstanceId;
            _entry = fragment.Definition.Entry;

            _coordinator = coordinator
                ?? throw new ArgumentNullException(nameof(coordinator));
        }

        public DialogueEvaluationStatus Evaluate(
            Guid initiatorInstanceId,
            Vector3 initiatorPosition,
            Vector3 speakerPosition)
        {
            return _coordinator.Evaluate(
                CreateRequest(
                    initiatorInstanceId,
                    initiatorPosition,
                    speakerPosition));
        }

        public UniTask<DialogueStartResult> StartDialogueAsync(
            Guid initiatorInstanceId,
            Vector3 initiatorPosition,
            Vector3 speakerPosition,
            CancellationToken cancellationToken)
        {
            return _coordinator.StartAsync(
                CreateRequest(
                    initiatorInstanceId,
                    initiatorPosition,
                    speakerPosition),
                cancellationToken);
        }

        private DialogueRequest CreateRequest(
            Guid initiatorInstanceId,
            Vector3 initiatorPosition,
            Vector3 speakerPosition)
        {
            return new DialogueRequest(
                initiatorInstanceId,
                _speakerInstanceId,
                initiatorPosition,
                speakerPosition,
                _entry);
        }
    }
}