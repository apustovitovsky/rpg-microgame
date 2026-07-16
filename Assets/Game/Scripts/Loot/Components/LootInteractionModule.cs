using Game.Commands;
using Game.Core;
using Game.Interaction;
using UnityEngine;
using VContainer;

namespace Game.Loot
{
    [DisallowMultipleComponent]
    public sealed class LootInteractionModule :
        MonoBehaviour,
        IModuleInstaller
    {
        [SerializeField] private Transform _interactionAnchor;

        [field: SerializeField]
        public float MaxRange { get; private set; } = 5f;

        public void Install(
            IContainerBuilder builder)
        {
            var interactionAnchor = _interactionAnchor != null
                ? _interactionAnchor
                : transform;

            builder.RegisterInstance(
                new LootInteractionSettings(
                    interactionAnchor,
                    MaxRange));

            builder.Register<LootInteraction>(Lifetime.Scoped)
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