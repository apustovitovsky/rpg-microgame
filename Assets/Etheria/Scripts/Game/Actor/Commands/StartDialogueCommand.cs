namespace Etheria.Game.Commands
{
    public readonly struct StartDialogueCommand : IActorCommand
    {
        public StartDialogueCommand(
            string actorId,
            string targetActorId)
        {
            ActorId = actorId;
            TargetActorId = targetActorId;
        }

        public string ActorId { get; }

        public string TargetActorId { get; }
    }
}