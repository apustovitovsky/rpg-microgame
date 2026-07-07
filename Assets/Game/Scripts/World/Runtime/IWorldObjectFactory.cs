namespace Game.World
{
    public interface IWorldObjectFactory<in TRequest>
    {
        IWorldObject Create(TRequest request);
    }
}