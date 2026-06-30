namespace Etheria.Game.Commands
{
    public readonly struct ActorCommandResult
    {
        public static ActorCommandResult Success =>
            new(true, ActorCommandFailureReason.None);

        public static ActorCommandResult Failed(
            ActorCommandFailureReason reason)
        {
            return new ActorCommandResult(
                false,
                reason);
        }

        private ActorCommandResult(
            bool succeeded,
            ActorCommandFailureReason failureReason)
        {
            Succeeded = succeeded;
            FailureReason = failureReason;
        }

        public bool Succeeded { get; }

        public ActorCommandFailureReason FailureReason { get; }
    }
}