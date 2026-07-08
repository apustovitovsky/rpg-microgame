namespace Game.World
{
    public interface IWorldObjectFactory<in TRequest>
    {
        WorldSpawnResult Create(TRequest request);
    }
}