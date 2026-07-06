using Game.World;

namespace Game.Actor
{
    public interface IActorIdentity
    {
        WorldId WorldId { get; }
        string DisplayName { get; }

        void Initialize(
            WorldId worldId,
            string displayName);
    }
}