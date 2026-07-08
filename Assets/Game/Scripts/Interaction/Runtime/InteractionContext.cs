using Game.World;

namespace Game.Interaction
{
    public readonly struct InteractionContext
    {
        public InteractionContext(
            IWorldHandle interactor,
            WorldId targetWorldId)
        {
            Interactor = interactor;
            TargetWorldId = targetWorldId;
        }

        public IWorldHandle Interactor { get; }

        public WorldId TargetWorldId { get; }
    }
}