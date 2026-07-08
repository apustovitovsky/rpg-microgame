namespace Game.World
{
    public interface IWorldSpawner
    {
        IWorldObject Spawn<TRequest>(
            TRequest request,
            IWorldObjectFactory<TRequest> factory);
    }

    public sealed class WorldSpawner : IWorldSpawner
    {
        private readonly IWorldManager _world;

        public WorldSpawner(IWorldManager world)
        {
            _world = world;
        }

        public IWorldObject Spawn<TRequest>(
            TRequest request,
            IWorldObjectFactory<TRequest> factory)
        {
            if (factory == null)
                return null;

            var result = factory.Create(request);

            if (!result.IsValid)
            {
                result.Lifetime?.Dispose();
                return null;
            }

            if (!_world.Track(
                    result.WorldObject,
                    result.Lifetime))
            {
                return null;
            }

            return result.WorldObject;
        }
    }
}