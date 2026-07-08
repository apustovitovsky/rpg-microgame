using System.Collections.Generic;

namespace Game.World
{
    public sealed class CompositeRegistration : IRegistrationToken
    {
        private readonly List<IRegistrationToken> _items = new();
        private bool _isDisposed;

        public void Add(IRegistrationToken registration)
        {
            if (registration == null)
                return;

            if (_isDisposed)
            {
                registration.Dispose();
                return;
            }

            _items.Add(registration);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            for (var i = _items.Count - 1; i >= 0; i--)
                _items[i]?.Dispose();

            _items.Clear();
        }
    }
}