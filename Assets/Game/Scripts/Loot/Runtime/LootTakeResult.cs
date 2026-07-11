namespace Game.Loot
{
    public enum LootTakeResult
    {
        Succeeded = 0,
        InvalidRequest = 1,
        SessionNotFound = 2,
        SourceInventoryUnavailable = 3,
        LooterInventoryUnavailable = 4,
        SourceStackNotFound = 5,
        InsufficientAmount = 6,
        DestinationFull = 7,
    }
}