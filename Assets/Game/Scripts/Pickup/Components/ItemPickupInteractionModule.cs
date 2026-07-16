using Game.Commands;
using Game.Core;
using Game.Interaction;
using UnityEngine;
using VContainer;

namespace Game.Pickup
{
    [DisallowMultipleComponent]
    public sealed class ItemPickupInteractionModule :
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
                new ItemPickupInteractionSettings(
                    interactionAnchor,
                    MaxRange));

            builder.Register<ItemPickupInteraction>(
                    Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.RegisterCommandRoutes<
                InteractionRoutes>();

            builder.RegisterCommandRoute<
                InteractionRoutes,
                InteractCommand,
                InteractionResult>();
        }
    }
}