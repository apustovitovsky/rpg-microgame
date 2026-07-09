using System;

namespace Game.World
{
    public sealed class LifetimeToken : IDisposable
    {
        private Action _onDispose;
        private bool _isDisposed;

        public LifetimeToken(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            var onDispose = _onDispose;
            _onDispose = null;

            onDispose?.Invoke();
        }
    }
}