using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Game.Core
{
    public sealed class AsyncLeaseGroup :
        IUniTaskAsyncDisposable
    {
        private readonly List<IUniTaskAsyncDisposable> _leases =
            new();

        private bool _isDisposed;

        public static AsyncLeaseGroup Combine(
            params IUniTaskAsyncDisposable[] leases)
        {
            var group = new AsyncLeaseGroup();

            foreach (var lease in leases)
                group.Add(lease);

            return group;
        }

        public void Add(
            IUniTaskAsyncDisposable lease)
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