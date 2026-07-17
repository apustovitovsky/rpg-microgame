using System;
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

        public bool IsReadyFor(
            Guid sessionId)
        {
            return _context.HasValue &&
                   sessionId != Guid.Empty &&
                   _context.Value.SessionId == sessionId &&
                   _readySessionId == sessionId;
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

            ContextEntered?.Invoke(context);

            return true;
        }

        public bool TryMarkReady(
            Guid sessionId)
        {
            if (!_context.HasValue ||
                sessionId == Guid.Empty ||
                _context.Value.SessionId != sessionId)
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

            _context = null;
            _readySessionId = Guid.Empty;

            ContextExited?.Invoke(context);

            return true;
        }
    }
}