using Game.World;

namespace Game.Interaction
{
    public readonly struct InteractionContext
    {
        public InteractionContext(
            WorldId interactorWorldId,
            WorldId targetWorldId)
        {
            InteractorWorldId = interactorWorldId;
            TargetWorldId = targetWorldId;
        }

        public WorldId InteractorWorldId { get; }

        public WorldId TargetWorldId { get; }
    }
}