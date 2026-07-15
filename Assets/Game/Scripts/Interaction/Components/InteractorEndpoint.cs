using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Interaction
{
    [DisallowMultipleComponent]
    public sealed class InteractorEndpoint :
        MonoBehaviour,
        IInteractor,
        IPrefabInstaller
    {
        [SerializeField]
        private Transform _interactionOrigin;

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