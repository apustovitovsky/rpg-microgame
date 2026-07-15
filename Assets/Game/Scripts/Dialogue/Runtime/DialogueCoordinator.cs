using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Dialogue
{
    public sealed class DialogueCoordinator :
        IDialogueCoordinator
    {
        private readonly IDialogueExecutor _executor;

        private DialogueSession _activeSession;

        public DialogueCoordinator(
            IDialogueExecutor executor)
        {
            _executor = executor
                ?? throw new ArgumentNullException(nameof(executor));
        }

        public bool TryGetActive(
            out DialogueSession session)
        {
            session = _activeSession;
            return session != null;
        }

        public DialogueEvaluationStatus Evaluate(
            DialogueRequest request)
        {
            if (!request.IsValid)
            {
                return DialogueEvaluationStatus.InvalidRequest;
            }

            if (_activeSession != null)
            {
                return DialogueEvaluationStatus.Busy;
            }

            return DialogueEvaluationStatus.Available;
        }

        public UniTask<DialogueStartResult> StartAsync(
            DialogueRequest request,
            CancellationToken cancellationToken)
        {
            var evaluation = Evaluate(request);

            if (evaluation != DialogueEvaluationStatus.Available)
            {
                return UniTask.FromResult(
                    DialogueStartResult.Rejected(evaluation));
            }

            var session = new DialogueSession(
                Guid.NewGuid(),
                request);

            _activeSession = session;

            RunSessionAsync(
                session,
                cancellationToken).Forget();

            return UniTask.FromResult(
                DialogueStartResult.Started(session.Id));
        }

        private async UniTask RunSessionAsync(
            DialogueSession session,
            CancellationToken cancellationToken)
        {
            try
            {
                await _executor.ExecuteAsync(
                    session,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
            }
            finally
            {
                if (_activeSession == session)
                {
                    _activeSession = null;
                }
            }
        }
    }
}