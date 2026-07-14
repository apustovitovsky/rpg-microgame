namespace Game.Actor
{
    public interface IActorSpawner
    {
        ActorInstance Spawn(ActorSpawnRequest request);
    }
}