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

            var settings = new DialogueInteractionSettings(
                MaxRange,
                interactionPoint);

            builder.RegisterInstance(settings);

            builder.Register<DialogueInteraction>(
                    Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.RegisterBinding<IInteractionTarget>();
        }
    }
}