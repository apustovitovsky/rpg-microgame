using System;
using VContainer.Unity;

namespace Game.Core
{
    public sealed class RegistryBinding<T> :
        IInitializable,
        IDisposable
        where T : class
    {
        private readonly IInstanceIdentity _identity;
        private readonly T _value;
        private readonly IRegistryWriter<T> _writer;

        public RegistryBinding(
            IInstanceIdentity identity,
            T value,
            IRegistryWriter<T> writer)
        {
            _identity = identity
                ?? throw new ArgumentNullException(nameof(identity));

            _value = value
                ?? throw new ArgumentNullException(nameof(value));

            _writer = writer
                ?? throw new ArgumentNullException(nameof(writer));
        }

        public void Initialize()
        {
            _writer.Add(
                _identity.InstanceId,
                _value);
        }

        public void Dispose()
        {
            _writer.Remove(
                _identity.InstanceId,
                _value);
        }
    }
}