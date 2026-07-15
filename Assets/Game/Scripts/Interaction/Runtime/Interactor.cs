using UnityEngine;

namespace Game.Interaction
{
    public sealed class Interactor :
        IInteractor
    {
        private readonly InteractorSettings _settings;

        public Interactor(
            InteractorSettings settings)
        {
            _settings = settings;
        }

        public Vector3 InteractionOrigin =>
            _settings.InteractionOrigin.position;

        public float MaxRange =>
            _settings.MaxRange;
    }

    public readonly struct InteractorSettings
    {
        public InteractorSettings(
            Transform interactionOrigin,
            float maxRange)
        {
            InteractionOrigin = interactionOrigin;
            MaxRange = maxRange;
        }

        public Transform InteractionOrigin { get; }

        public float MaxRange { get; }
    }
}