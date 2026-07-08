using Game.World;

namespace Game.Interaction
{
    public readonly struct InteractionContext
    {
        public InteractionContext(
            IWorldObject interactor,
            WorldId targetWorldId)
        {
            Interactor = interactor;
            TargetWorldId = targetWorldId;
        }

        public IWorldObject Interactor { get; }

        public WorldId TargetWorldId { get; }
    }
}