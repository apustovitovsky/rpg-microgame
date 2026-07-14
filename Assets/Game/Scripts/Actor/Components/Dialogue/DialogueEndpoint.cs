using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using Game.Interaction;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class DialogueEndpoint :
        MonoBehaviour,
        IInteractable,
        IDialogueSessionStarter,
        IPrefabInstaller
    {
        [SerializeField] private Transform _interactionPoint;

        [field: SerializeField]
        public float MaxRange { get; private set; } = 5f;

        public Vector3 InteractionPoint =>
            _interactionPoint != null
                ? _interactionPoint.position
                : transform.position;

        public void Install(
            IContainerBuilder builder)
        {
            builder.RegisterComponent(this)
                .AsSelf()
                .As<IInteractable>()
                .As<IDialogueSessionStarter>();

            builder.Register<InteractCommandHandler>(Lifetime.Scoped)
                .AsImplementedInterfaces();
        }

        public bool CanInteract(InteractionContext context)
        {
            return context.InteractorInstanceId != Guid.Empty &&
                   context.TargetInstanceId != Guid.Empty &&
                   context.InteractorInstanceId !=
                   context.TargetInstanceId;
        }

        public UniTask InteractAsync(
            InteractionContext context,
            CancellationToken token)
        {
            return StartDialogueAsync(
                context.InteractorInstanceId,
                token);
        }

        public UniTask StartDialogueAsync(
            Guid interactorInstanceId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Log(
                $"Dialogue started by actor '{interactorInstanceId:N}'.",
                this);

            return UniTask.CompletedTask;
        }
    }
}