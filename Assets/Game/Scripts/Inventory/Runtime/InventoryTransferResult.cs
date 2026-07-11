namespace Game.Inventory
{
    public enum InventoryTransferResult
    {
        Succeeded = 0,
        InvalidRequest = 1,
        SourceStackNotFound = 2,
        InsufficientAmount = 3,
        DestinationFull = 4,
    }
}