namespace Game.Commands
{
    public readonly struct CommandDispatchResult
    {
        public CommandDispatchResult(
            CommandDispatchStatus status)
        {
            Status = status;
        }

        public CommandDispatchStatus Status { get; }

        public bool IsDelivered =>
            Status == CommandDispatchStatus.Delivered;
    }

    public readonly struct CommandDispatchResult<TResult>
    {
        public CommandDispatchResult(
            CommandDispatchStatus status,
            TResult value)
        {
            Status = status;
            Value = value;
        }

        public CommandDispatchStatus Status { get; }

        public TResult Value { get; }

        public bool IsDelivered =>
            Status == CommandDispatchStatus.Delivered;
    }
}