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
            _executor = executor;
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

        public async UniTask<DialogueRunResult> RunAsync(
            DialogueRequest request,
            CancellationToken cancellationToken)
        {
            var evaluation = Evaluate(request);

            if (evaluation != DialogueEvaluationStatus.Available)
            {
                return DialogueRunResult.Rejected(evaluation);
            }

            var session = new DialogueSession(
                Guid.NewGuid(),
                request);

            _activeSession = session;

            try
            {
                await _executor.ExecuteAsync(
                    session,
                    cancellationToken);

                return DialogueRunResult.Completed(session.Id);
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