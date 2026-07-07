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

            var worldObject = factory.Create(request);

            if (worldObject == null)
                return null;

            if (!_world.Register(worldObject))
            {
                _world.Despawn(worldObject.WorldId);
                return null;
            }

            return worldObject;
        }
    }
}