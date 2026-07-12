using System;
using VContainer.Unity;

namespace Game.Core
{
    public interface IRegistryBindingSource<out T>
        where T : class
    {
        Guid Id { get; }
        T Value { get; }
    }

    public sealed class RegistryBinding<T> :
        IInitializable,
        IDisposable
        where T : class
    {
        private readonly IRegistryBindingSource<T> _source;
        private readonly IRegistryWriter<T> _writer;

        public RegistryBinding(
            IRegistryBindingSource<T> source,
            IRegistryWriter<T> writer)
        {
            _source = source;
            _writer = writer;
        }

        public void Initialize()
        {
            _writer.Add(_source.Id, _source.Value);
        }

        public void Dispose()
        {
            _writer.Remove(_source.Id, _source.Value);
        }
    }
}