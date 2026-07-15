using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Interaction
{
    [DisallowMultipleComponent]
    public sealed class InteractorEndpoint :
        MonoBehaviour,
        IInteractionSource,
        IModuleInstaller
    {
        [SerializeField]
        private Transform _interactionOrigin;

        [field: SerializeField]
        public float MaxRange { get; private set; } = 5f;

        public Vector3 InteractionOrigin =>
            _interactionOrigin != null
                ? _interactionOrigin.position
                : transform.position;

        public void Install(
            IContainerBuilder builder)
        {
            builder.RegisterComponent(this)
                .AsImplementedInterfaces();

            builder.Register<InteractCommandHandler>(
                    Lifetime.Scoped)
                .AsImplementedInterfaces();
        }
    }
}