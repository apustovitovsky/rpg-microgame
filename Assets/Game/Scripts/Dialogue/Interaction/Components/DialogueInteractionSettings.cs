using UnityEngine;

namespace Game.Dialogue.Interaction
{
    public readonly struct DialogueInteractionSettings
    {
        public DialogueInteractionSettings(
            float maxRange,
            Transform interactionPoint)
        {
            MaxRange = maxRange;
            InteractionPoint = interactionPoint;
        }

        public float MaxRange { get; }

        public Transform InteractionPoint { get; }
    }
}