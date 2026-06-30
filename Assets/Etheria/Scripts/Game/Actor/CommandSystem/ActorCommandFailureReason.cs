namespace Etheria.Game.Commands
{
    public enum ActorCommandFailureReason
    {
        None = 0,
        InvalidCommand = 1,
        ActorNotFound = 2,
        Blocked = 3,
        Busy = 4,
        Cancelled = 5,
        Failed = 6
    }
}