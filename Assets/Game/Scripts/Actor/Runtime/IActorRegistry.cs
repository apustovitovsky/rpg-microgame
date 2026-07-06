namespace Game.Actor
{
    public interface IActorRegistry
    {
        bool TryGet(
            string actorId,
            out ActorInstance actor);
    }

    public interface IActorRegistryWriter
    {
        void Register(ActorInstance actor);

        void Unregister(ActorInstance actor);
    }
}