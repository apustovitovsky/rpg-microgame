using System.Threading;
using Cysharp.Threading.Tasks;
using Game.CommandSystem;

namespace Game.Actor
{
    public sealed class AttackCommandHandler :
        CommandHandler<AttackCommand>
    {
        private readonly IActorRegistry _actors;
        private readonly IActionGate _actionGate;

        public AttackCommandHandler(
            IActorRegistry actors,
            IActionGate actionGate)
        {
            _actors = actors;
            _actionGate = actionGate;
        }

        public override async UniTask<CommandStatus> HandleAsync(
            AttackCommand command,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.ActorId) ||
                string.IsNullOrWhiteSpace(command.TargetActorId))
            {
                return CommandStatus.InvalidCommand;
            }

            if (_actors == null ||
                !_actors.TryGet(command.ActorId, out var actor))
            {
                return CommandStatus.ActorNotFound;
            }

            if (!actor.TryGet<IActorCombatHandler>(out var combat))
            {
                return CommandStatus.HandlerNotFound;
            }

            if (_actionGate == null ||
                !_actionGate.TryEnter(
                    command.ActorId,
                    ActorActionChannel.Combat,
                    ActorActionChannel.Locomotion |
                    ActorActionChannel.Dialogue |
                    ActorActionChannel.Interaction,
                    out var scope))
            {
                return CommandStatus.Blocked;
            }

            using (scope)
            {
                return await combat.AttackAsync(
                    command.TargetActorId,
                    cancellationToken);
            }
        }
    }
}