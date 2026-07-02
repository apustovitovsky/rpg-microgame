namespace Game.CommandSystem
{
    public enum CommandStatus
    {
        Succeeded = 0,
        InvalidCommand = 1,
        HandlerNotFound = 2,
        ActorNotFound = 3,
        Blocked = 4,
        Interrupted = 5,
        Busy = 6,
        Cancelled = 7,
        Failed = 8
    }
}