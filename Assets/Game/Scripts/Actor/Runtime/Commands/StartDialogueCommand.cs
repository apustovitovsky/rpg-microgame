using Game.CommandSystem;

namespace Game.Actor
{
    public readonly struct StartDialogueCommand : ICommand
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