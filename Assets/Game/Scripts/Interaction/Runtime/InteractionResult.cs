namespace Game.Interaction
{
    public enum InteractionStatus
    {
        Rejected = 0,
        Completed = 1,
        Busy = 2
    }

    public readonly struct InteractionResult
    {
        public InteractionResult(
            InteractionStatus status)
        {
            Status = status;
        }

        public InteractionStatus Status { get; }

        public bool Succeeded =>
            Status == InteractionStatus.Completed;

        public static InteractionResult Rejected =>
            new(InteractionStatus.Rejected);

        public static InteractionResult Completed =>
            new(InteractionStatus.Completed);

        public static InteractionResult Busy =>
            new(InteractionStatus.Busy);
    }
}