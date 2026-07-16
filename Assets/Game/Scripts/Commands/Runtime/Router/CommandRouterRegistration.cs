using System;
using Game.Core;
using VContainer.Unity;

namespace Game.Commands
{
    public sealed class CommandRouterRegistration :
        IInitializable,
        IDisposable
    {
        private readonly IInstanceIdentity _identity;
        private readonly ICommandRouter _router;
        private readonly ICommandRouterRegistrar _registrar;

        public CommandRouterRegistration(
            IInstanceIdentity identity,
            ICommandRouter router,
            ICommandRouterRegistrar registrar)
        {
            _identity = identity
                ?? throw new ArgumentNullException(nameof(identity));

            _router = router
                ?? throw new ArgumentNullException(nameof(router));

            _registrar = registrar
                ?? throw new ArgumentNullException(
                    nameof(registrar));
        }

        public void Initialize()
        {
            _registrar.Register(
                _identity.InstanceId,
                _router);
        }

        public void Dispose()
        {
            _registrar.Unregister(
                _identity.InstanceId,
                _router);
        }
    }
}