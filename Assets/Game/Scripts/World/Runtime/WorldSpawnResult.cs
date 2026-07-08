namespace Game.World
{
    public readonly struct WorldSpawnResult
    {
        public WorldSpawnResult(
            IWorldObject worldObject,
            IRegistrationToken lifetime)
        {
            WorldObject = worldObject;
            Lifetime = lifetime;
        }

        public IWorldObject WorldObject { get; }

        public IRegistrationToken Lifetime { get; }

        public bool IsValid =>
            WorldObject != null &&
            !WorldObject.WorldId.IsEmpty;
    }
}