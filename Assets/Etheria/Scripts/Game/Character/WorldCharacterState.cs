namespace Etheria.Game.Character
{
    public sealed class WorldCharacterState
    {
        public string CharacterId { get; }
        public string LocationId { get; }
        public string AnchorKey { get; }
        public bool IsAlive { get; }
        public bool IsPresent { get; }

        public WorldCharacterState(
            string characterId,
            string locationId,
            string anchorKey,
            bool isAlive,
            bool isPresent)
        {
            CharacterId = characterId;
            LocationId = locationId;
            AnchorKey = anchorKey;
            IsAlive = isAlive;
            IsPresent = isPresent;
        }
    }
}
