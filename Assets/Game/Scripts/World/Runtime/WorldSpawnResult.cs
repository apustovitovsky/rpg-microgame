namespace Game.World
{
    public readonly struct WorldSpawnResult
    {
        public WorldSpawnResult(IWorldLifetime lifetime)
        {
            Lifetime = lifetime;
        }

        public IWorldLifetime Lifetime { get; }

        public WorldId WorldId => Lifetime?.WorldId ?? default;

        public bool IsValid =>
            Lifetime != null &&
            !Lifetime.WorldId.IsEmpty &&
            !Lifetime.IsDisposed;
    }
}