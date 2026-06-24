namespace Etheria.Game.Character
{
    public sealed class WorldCharacterState
    {
        public string CharacterId { get; }
        public string LocationId { get; }
        public bool IsAlive { get; }

        public bool IsPresent { get; }

        public WorldCharacterState(
            string characterId,
            string locationId,
            bool isAlive,
            bool isPresent)
        {
            CharacterId = characterId;
            LocationId = locationId;
            IsAlive = isAlive;
            IsPresent = isPresent;
        }
    }
}
