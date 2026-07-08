using System;

namespace Game.World
{
    public interface IRegistrationToken : IDisposable
    {
    }

    public sealed class RegistrationToken : IRegistrationToken
    {
        private Action _onDispose;
        private bool _isDisposed;

        public RegistrationToken(Action onDispose)
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