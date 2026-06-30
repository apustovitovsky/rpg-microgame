using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Game.Commands;
using Etheria.Game.Npc;
using Etheria.Game.World;

namespace Etheria.Actor
{
    public sealed class ActorCommandService : IActorCommandService
    {
        private readonly INpcRuntimeRegistry _npcRuntimes;
        private readonly IActorActionGate _actionGate;

        public ActorCommandService(
            INpcRuntimeRegistry npcRuntimes,
            IActorActionGate actionGate)
        {
            _npcRuntimes = npcRuntimes;
            _actionGate = actionGate;
        }

        public UniTask<ActorCommandResult> ExecuteAsync(
            IActorCommand command,
            CancellationToken cancellationToken)
        {
            if (command == null)
            {
                return UniTask.FromResult(
                    ActorCommandResult.Failed(
                        ActorCommandFailureReason.InvalidCommand));
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return UniTask.FromResult(
                    ActorCommandResult.Failed(
                        ActorCommandFailureReason.Cancelled));
            }

            return command switch
            {
                StartDialogueCommand startDialogue =>
                    ExecuteStartDialogue(
                        startDialogue,
                        cancellationToken),

                MoveActorToLocationCommand moveToLocation =>
                    ExecuteMoveToLocation(
                        moveToLocation,
                        cancellationToken),

                _ =>
                    UniTask.FromResult(
                        ActorCommandResult.Failed(
                            ActorCommandFailureReason.InvalidCommand))
            };
        }

        private async UniTask<ActorCommandResult> ExecuteStartDialogue(
            StartDialogueCommand command,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.ActorId))
            {
                return ActorCommandResult.Failed(
                    ActorCommandFailureReason.InvalidCommand);
            }

            if (!_npcRuntimes.TryGet(
                    command.ActorId,
                    out var runtime))
            {
                return ActorCommandResult.Failed(
                    ActorCommandFailureReason.ActorNotFound);
            }

            if (runtime.DialogueStarter == null ||
                !runtime.DialogueStarter.CanStartDialogue)
            {
                return ActorCommandResult.Failed(
                    ActorCommandFailureReason.Blocked);
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
                return ActorCommandResult.Failed(
                    ActorCommandFailureReason.Blocked);
            }

            using (scope)
            {
                return await runtime.DialogueStarter.StartDialogueAsync(
                    cancellationToken);
            }
        }

        private async UniTask<ActorCommandResult> ExecuteMoveToLocation(
            MoveActorToLocationCommand command,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.ActorId) ||
                string.IsNullOrWhiteSpace(command.LocationId))
            {
                return ActorCommandResult.Failed(
                    ActorCommandFailureReason.InvalidCommand);
            }

            if (!_npcRuntimes.TryGet(
                    command.ActorId,
                    out var runtime))
            {
                return ActorCommandResult.Failed(
                    ActorCommandFailureReason.ActorNotFound);
            }

            if (runtime.Travel == null)
            {
                return ActorCommandResult.Failed(
                    ActorCommandFailureReason.Failed);
            }

            if (_actionGate == null ||
                !_actionGate.TryEnter(
                    command.ActorId,
                    ActorActionChannel.Locomotion,
                    ActorActionChannel.Locomotion |
                    ActorActionChannel.Combat,
                    out var scope))
            {
                return ActorCommandResult.Failed(
                    ActorCommandFailureReason.Blocked);
            }

            using (scope)
            {
                var completion =
                    new UniTaskCompletionSource<bool>();

                if (!runtime.Travel.TryMoveToLocation(
                        command.LocationId,
                        string.IsNullOrWhiteSpace(command.AnchorKey)
                            ? NavigationAnchorKeys.Default
                            : command.AnchorKey,
                        command.Filter,
                        arrived => completion.TrySetResult(arrived)))
                {
                    return ActorCommandResult.Failed(
                        ActorCommandFailureReason.Failed);
                }

                var arrived = await completion.Task
                    .AttachExternalCancellation(cancellationToken);

                return arrived
                    ? ActorCommandResult.Success
                    : ActorCommandResult.Failed(
                        ActorCommandFailureReason.Failed);
            }
        }
    }
}