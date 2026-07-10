using System;
using Game.World;

namespace Game.UI
{
    public interface IDisplayNameProvider
    {
        string DisplayName { get; }
    }

    public interface IDisplayNameService
    {
        bool TryGet(
            Guid instanceId,
            out string displayName);
    }

    public interface IDisplayNameRegistrationService
    {
        IDisposable Register(
            Guid instanceId,
            IDisplayNameProvider provider);
    }

    public sealed class DisplayNameProvider :
        IDisplayNameProvider
    {
        private readonly Func<string> _getDisplayName;

        public DisplayNameProvider(
            Func<string> getDisplayName)
        {
            _getDisplayName = getDisplayName
                ?? throw new ArgumentNullException(
                    nameof(getDisplayName));
        }

        public string DisplayName => _getDisplayName();
    }

    public sealed class DisplayNameService :
        IDisplayNameService,
        IDisplayNameRegistrationService
    {
        private readonly InstanceIndex<IDisplayNameProvider> _providers =
            new();

        public IDisposable Register(
            Guid instanceId,
            IDisplayNameProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            return _providers.Register(instanceId, provider);
        }

        public bool TryGet(
            Guid instanceId,
            out string displayName)
        {
            displayName = null;

            if (!_providers.TryGet(
                    instanceId,
                    out var provider))
            {
                return false;
            }

            displayName = provider.DisplayName;

            return !string.IsNullOrWhiteSpace(displayName);
        }
    }
}