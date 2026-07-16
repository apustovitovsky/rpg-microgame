namespace Game.Commands
{
    public enum CommandDispatchStatus
    {
        Delivered = 0,
        TargetNotFound = 1,
        Unsupported = 2,
        Dropped = 3,
        Cancelled = 4,
        Failed = 5,
    }
}