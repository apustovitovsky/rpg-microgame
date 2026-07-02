using System.Threading;
using Cysharp.Threading.Tasks;
using Game.CommandSystem;

namespace Game.Actor
{
    public sealed class MoveToLocationCommandHandler :
        CommandHandler<MoveToLocationCommand>
    {
        private readonly IActorRegistry _actors;
        private readonly IActionGate _actionGate;

        public MoveToLocationCommandHandler(
            IActorRegistry actors,
            IActionGate actionGate)
        {
            _actors = actors;
            _actionGate = actionGate;
        }

        public override async UniTask<CommandStatus> HandleAsync(
            MoveToLocationCommand command,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.ActorId) ||
                string.IsNullOrWhiteSpace(command.LocationId) ||
                string.IsNullOrWhiteSpace(command.AnchorKey))
            {
                return CommandStatus.InvalidCommand;
            }

            if (_actors == null ||
                !_actors.TryGet(command.ActorId, out var actor))
            {
                return CommandStatus.ActorNotFound;
            }

            if (!actor.TryGet<IActorTravelHandler>(out var travel))
            {
                return CommandStatus.HandlerNotFound;
            }

            if (_actionGate == null ||
                !_actionGate.TryEnter(
                    command.ActorId,
                    ActorActionChannel.Locomotion,
                    ActorActionChannel.Combat |
                    ActorActionChannel.Interaction |
                    ActorActionChannel.Dialogue,
                    out var scope))
            {
                return CommandStatus.Blocked;
            }

            using (scope)
            {
                return await travel.MoveToLocationAsync(
                    command.LocationId,
                    command.AnchorKey,
                    cancellationToken);
            }
        }
    }
}