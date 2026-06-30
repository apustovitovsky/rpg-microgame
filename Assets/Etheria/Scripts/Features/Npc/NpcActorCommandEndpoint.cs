using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Game.Commands;
using Etheria.Game.Npc;
using Etheria.Game.World;
using UnityEngine;
using VContainer;

namespace Etheria.Npc
{
    public sealed class NpcActorCommandEndpoint :
        MonoBehaviour,
        IActorCommandEndpoint
    {
        private IActorCommandService _commands;
        private INpcIdentity _identity;

        [Inject]
        public void Construct(
            IActorCommandService commands,
            INpcIdentity identity)
        {
            _commands = commands;
            _identity = identity;
        }

        public UniTask<ActorCommandResult> StartDialogueAsync(
            string targetActorId,
            CancellationToken cancellationToken)
        {
            if (!CanExecuteCommand())
            {
                return UniTask.FromResult(
                    ActorCommandResult.Failed(
                        ActorCommandFailureReason.ActorNotFound));
            }

            return _commands.ExecuteAsync(
                new StartDialogueCommand(
                    _identity.NpcId,
                    targetActorId ?? string.Empty),
                cancellationToken);
        }

        public UniTask<ActorCommandResult> MoveToLocationAsync(
            string locationId,
            string anchorKey,
            NavigationQueryFilter filter,
            CancellationToken cancellationToken)
        {
            if (!CanExecuteCommand() ||
                string.IsNullOrWhiteSpace(locationId))
            {
                return UniTask.FromResult(
                    ActorCommandResult.Failed(
                        ActorCommandFailureReason.InvalidCommand));
            }

            return _commands.ExecuteAsync(
                new MoveActorToLocationCommand(
                    _identity.NpcId,
                    locationId,
                    anchorKey,
                    filter),
                cancellationToken);
        }

        private bool CanExecuteCommand()
        {
            return _commands != null &&
                   _identity != null &&
                   !string.IsNullOrWhiteSpace(_identity.NpcId);
        }
    }
}