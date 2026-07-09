namespace Game.World
{
    public readonly struct WorldInfo
    {
        public WorldInfo(
            WorldId worldId,
            string displayName)
        {
            WorldId = worldId;
            DisplayName = displayName;
        }

        public WorldId WorldId { get; }

        public string DisplayName { get; }
    }
}