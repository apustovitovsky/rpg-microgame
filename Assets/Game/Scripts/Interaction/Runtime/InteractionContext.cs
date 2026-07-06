using Game.World;

namespace Game.Interaction
{
    public readonly struct InteractionContext
    {
        public InteractionContext(
            IWorldObject interactor,
            IWorldObject target)
        {
            Interactor = interactor;
            Target = target;
        }

        public IWorldObject Interactor { get; }
        public IWorldObject Target { get; }
    }
}