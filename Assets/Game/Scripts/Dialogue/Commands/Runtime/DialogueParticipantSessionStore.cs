using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Game.Dialogue.Commands
{
    public sealed class DialogueParticipantSessionStore
    {
        private readonly Dictionary<Guid, IDialogueParticipantLease>
            _leases = new();

        public bool Contains(
            Guid sessionId)
        {
            return _leases.ContainsKey(sessionId);
        }

        public void Add(
            Guid sessionId,
            IDialogueParticipantLease lease)
        {
            if (sessionId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Dialogue session id is required.",
                    nameof(sessionId));
            }

            if (lease == null)
                throw new ArgumentNullException(nameof(lease));

            _leases.Add(sessionId, lease);
        }

        public bool TryTake(
            Guid sessionId,
            out IDialogueParticipantLease lease)
        {
            if (!_leases.TryGetValue(sessionId, out lease))
                return false;

            _leases.Remove(sessionId);
            return true;
        }
    }

    public sealed class CompositeDialogueParticipantLease :
        IDialogueParticipantLease
    {
        private readonly List<IDialogueParticipantLease> _leases =
            new();

        private bool _isDisposed;

        public void Add(
            IDialogueParticipantLease lease)
        {
            if (lease == null)
                throw new ArgumentNullException(nameof(lease));

            if (_isDisposed)
            {
                throw new InvalidOperationException(
                    "Cannot add a lease after disposal.");
            }

            _leases.Add(lease);
        }

        public async UniTask DisposeAsync()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            List<Exception> failures = null;

            for (var index = _leases.Count - 1;
                 index >= 0;
                 index--)
            {
                try
                {
                    await _leases[index].DisposeAsync();
                }
                catch (Exception exception)
                {
                    failures ??= new List<Exception>();
                    failures.Add(exception);
                }
            }

            _leases.Clear();

            if (failures == null)
                return;

            if (failures.Count == 1)
                throw failures[0];

            throw new AggregateException(failures);
        }
    }
}