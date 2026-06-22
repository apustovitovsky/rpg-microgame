namespace Etheria.Game.Character
{
    public sealed class WorldCharacterState
    {
        public string CharacterId { get; }
        public string SpawnPointId { get; }
        public bool IsAlive { get; }

        public WorldCharacterState(
            string characterId,
            string spawnPointId,
            bool isAlive)
        {
            CharacterId = characterId;
            SpawnPointId = spawnPointId;
            IsAlive = isAlive;
        }
    }
}