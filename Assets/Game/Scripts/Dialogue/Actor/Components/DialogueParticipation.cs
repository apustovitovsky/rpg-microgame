using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Dialogue;
using UnityEngine;

namespace Game.Dialogue.Actor
{
    [DisallowMultipleComponent]
    public sealed class DialogueParticipation :
        MonoBehaviour,
        IDialogueParticipation
    {
        private DialogueSessionContext? _context;
        private Guid _readySessionId;
        private UniTaskCompletionSource _readyCompletion;

        public event Action<DialogueSessionContext>
            ContextEntered;

        public event Action<DialogueSessionContext>
            ContextExited;

        public bool HasContext =>
            _context.HasValue;

        public bool TryGetContext(
            out DialogueSessionContext context)
        {
            if (_context.HasValue)
            {
                context = _context.Value;
                return true;
            }

            context = default;
            return false;
        }

        public bool TryEnter(
            DialogueSessionContext context)
        {
            if (context.SessionId == Guid.Empty ||
                context.OtherParticipantInstanceId == Guid.Empty)
            {
                return false;
            }

            if (_context.HasValue)
            {
                return _context.Value.SessionId ==
                           context.SessionId &&
                       _context.Value.OtherParticipantInstanceId ==
                           context.OtherParticipantInstanceId;
            }

            _context = context;
            _readySessionId = Guid.Empty;
            _readyCompletion =
                new UniTaskCompletionSource();

            ContextEntered?.Invoke(context);

            return true;
        }

        public async UniTask WaitUntilReadyAsync(
            Guid sessionId,
            CancellationToken cancellationToken)
        {
            if (!_context.HasValue ||
                sessionId == Guid.Empty ||
                _context.Value.SessionId != sessionId)
            {
                throw new InvalidOperationException(
                    "Dialogue session is not active.");
            }

            if (_readySessionId == sessionId)
            {
                return;
            }

            var readyCompletion = _readyCompletion;

            if (readyCompletion == null)
            {
                throw new InvalidOperationException(
                    "Dialogue readiness is not initialized.");
            }

            using (cancellationToken.Register(
                       () => readyCompletion.TrySetCanceled(
                           cancellationToken)))
            {
                await readyCompletion.Task;
            }
        }

        public bool TryMarkReady(
            Guid sessionId)
        {
            if (!_context.HasValue ||
                sessionId == Guid.Empty ||
                _context.Value.SessionId != sessionId ||
                _readyCompletion == null ||
                !_readyCompletion.TrySetResult())
            {
                return false;
            }

            _readySessionId = sessionId;

            return true;
        }

        public bool TryExit(
            Guid sessionId)
        {
            if (!_context.HasValue ||
                sessionId == Guid.Empty ||
                _context.Value.SessionId != sessionId)
            {
                return false;
            }

            var context = _context.Value;
            var readyCompletion = _readyCompletion;

            _context = null;
            _readySessionId = Guid.Empty;
            _readyCompletion = null;

            readyCompletion?.TrySetCanceled();

            ContextExited?.Invoke(context);

            return true;
        }
    }
}