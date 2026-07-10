namespace Game.Pickup
{
    public enum CollectResult
    {
        Succeeded = 0,
        InvalidCollector = 1,
        InvalidCollectable = 2,
        CannotCollect = 3,
        AlreadyInProgress = 4,
        Failed = 5
    }
}