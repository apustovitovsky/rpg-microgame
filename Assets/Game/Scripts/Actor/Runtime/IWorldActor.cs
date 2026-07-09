using Game.World;

namespace Game.Actor
{
    public interface IWorldActor
    {
        WorldId WorldId { get; }

        ActorDefinition Definition { get; }

        IActorView View { get; }

        IActorNavigation Navigation { get; }

        IActorDialogue Dialogue { get; }

        IActorInputBinder InputBinder { get; }
    }
}