using System.Threading;
using Cysharp.Threading.Tasks;
using Game.CommandSystem;

namespace Game.Actor
{
    public sealed class StartDialogueCommandHandler :
        CommandHandler<StartDialogueCommand>
    {
        private readonly IActorRegistry _actors;
        private readonly IActionGate _actionGate;

        public StartDialogueCommandHandler(
            IActorRegistry actors,
            IActionGate actionGate)
        {
            _actors = actors;
            _actionGate = actionGate;
        }

        public override async UniTask<CommandStatus> HandleAsync(
            StartDialogueCommand command,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.ActorId))
            {
                return CommandStatus.InvalidCommand;
            }

            if (_actors == null ||
                !_actors.TryGet(command.ActorId, out var actor))
            {
                return CommandStatus.ActorNotFound;
            }

            if (!actor.TryGet<IActorDialogueHandler>(out var dialogue))
            {
                return CommandStatus.HandlerNotFound;
            }

            if (_actionGate == null ||
                !_actionGate.TryEnter(
                    command.ActorId,
                    ActorActionChannel.Dialogue,
                    ActorActionChannel.Locomotion |
                    ActorActionChannel.Combat |
                    ActorActionChannel.Interaction,
                    out var scope))
            {
                return CommandStatus.Blocked;
            }

            using (scope)
            {
                return await dialogue.StartDialogueAsync(
                    command.TargetActorId,
                    cancellationToken);
            }
        }
    }
}