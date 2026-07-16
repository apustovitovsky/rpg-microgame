using Game.Commands;
using Game.Core;
using Game.Interaction;
using UnityEngine;
using VContainer;

namespace Game.Dialogue.Interaction
{
    [DisallowMultipleComponent]
    public sealed class DialogueInteractionModule :
        MonoBehaviour,
        IModuleInstaller
    {
        [SerializeField] private Transform _interactionPoint;

        [field: SerializeField]
        public float MaxRange { get; private set; } = 5f;

        public void Install(
            IContainerBuilder builder)
        {
            var interactionPoint = _interactionPoint != null
                ? _interactionPoint
                : transform;

            builder.RegisterInstance(
                new DialogueInteractionSettings(
                    MaxRange,
                    interactionPoint));

            builder.Register<DialogueInteraction>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.RegisterCommandExecutionGroup<
                InteractionExecution>();

            builder.RegisterCommandExecution<
                InteractionExecution,
                InteractCommand,
                InteractionResult>();
        }
    }
}