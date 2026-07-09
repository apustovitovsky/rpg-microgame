using Game.World;
using UnityEngine;

namespace Game.Interaction
{
    public readonly struct InteractionContext
    {
        public InteractionContext(
            WorldId interactorWorldId,
            Vector3 origin,
            WorldId targetWorldId)
        {
            InteractorWorldId = interactorWorldId;
            Origin = origin;
            TargetWorldId = targetWorldId;
        }

        public WorldId InteractorWorldId { get; }

        public Vector3 Origin { get; }

        public WorldId TargetWorldId { get; }
    }
}