using Game.World;

namespace Game.Actor
{
    public interface IWorldActor
    {
        WorldId WorldId { get; }
        ActorDefinition Definition { get; }
    }
}