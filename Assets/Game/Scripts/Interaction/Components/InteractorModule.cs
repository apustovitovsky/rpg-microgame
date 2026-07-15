using Game.Core;
using UnityEngine;
using VContainer;

namespace Game.Interaction
{
    [DisallowMultipleComponent]
    public sealed class InteractorModule :
        MonoBehaviour,
        IModuleInstaller
    {
        [SerializeField]
        private Transform _interactionOrigin;

        [field: SerializeField]
        public float MaxRange { get; private set; } = 5f;

        public void Install(
            IContainerBuilder builder)
        {
            var interactionOrigin = _interactionOrigin != null
                ? _interactionOrigin
                : transform;

            builder.RegisterInstance(
                new InteractorSettings(
                    interactionOrigin,
                    MaxRange));

            builder.Register<Interactor>(Lifetime.Scoped)
                .AsImplementedInterfaces();
        }
    }
}