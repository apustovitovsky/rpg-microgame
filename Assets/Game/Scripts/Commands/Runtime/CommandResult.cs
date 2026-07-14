namespace Game.Commands
{
    public enum CommandResult
    {
        Completed = 0,
        Rejected = 1,
        Busy = 2,
        Unsupported = 3,
        TargetNotFound = 4,
        Cancelled = 5,
        Failed = 6,
    }
}