namespace Game.World
{
    public readonly struct WorldSpawnResult
    {
        public WorldSpawnResult(
            IWorldHandle handle,
            IRegistrationToken lifetime)
        {
            Handle = handle;
            Lifetime = lifetime;
        }

        public IWorldHandle Handle { get; }

        public IRegistrationToken Lifetime { get; }

        public bool IsValid =>
            Handle != null &&
            !Handle.WorldId.IsEmpty;
    }
}