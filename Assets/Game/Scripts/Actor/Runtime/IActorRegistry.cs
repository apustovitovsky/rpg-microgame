namespace Game.Actor
{
    public interface IActorRegistry
    {
        bool TryGet(
            string actorId,
            out IActorView actor);
    }

    public interface IActorRegistryWriter
    {
        void Register(IActorView actor);

        void Unregister(IActorView actor);
    }
}