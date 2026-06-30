using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Game.Commands;
using Etheria.Game.Npc;

namespace Etheria.Npc
{
    public sealed class NpcDialogueStarter : INpcDialogueStarter
    {
        private readonly NpcDefinitionSO _definition;
        private readonly NpcDialogueSessionService _session;

        public NpcDialogueStarter(
            NpcDefinitionSO definition,
            NpcDialogueSessionService session)
        {
            _definition = definition;
            _session = session;
        }

        public bool CanStartDialogue =>
            _definition != null &&
            _session != null &&
            _session.CanStartDialogue(_definition.NpcId);

        public async UniTask<ActorCommandResult> StartDialogueAsync(
            CancellationToken cancellationToken)
        {
            if (_definition == null ||
                _session == null)
            {
                return ActorCommandResult.Failed(
                    ActorCommandFailureReason.Failed);
            }

            if (!_session.TryStartDialogue(_definition.NpcId))
            {
                return ActorCommandResult.Failed(
                    ActorCommandFailureReason.Blocked);
            }

            var isCompleted = false;

            _session.SetCompletedCallback(
                () => isCompleted = true);

            try
            {
                await UniTask.WaitUntil(
                    () => isCompleted,
                    cancellationToken: cancellationToken);

                return ActorCommandResult.Success;
            }
            catch (OperationCanceledException)
            {
                return ActorCommandResult.Failed(
                    ActorCommandFailureReason.Cancelled);
            }
            finally
            {
                _session.ClearCompletedCallback();
            }
        }

        public void Clear()
        {
            _session?.ClearCompletedCallback();
        }
    }
}