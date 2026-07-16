using System;
using Game.Core;
using VContainer.Unity;

namespace Game.Commands
{
    public sealed class CommandRouterBinding :
        IInitializable,
        IDisposable
    {
        private readonly IInstanceIdentity _identity;
        private readonly ICommandRouter _router;
        private readonly ICommandRouterRegistration _registration;

        public CommandRouterBinding(
            IInstanceIdentity identity,
            ICommandRouter router,
            ICommandRouterRegistration registration)
        {
            _identity = identity
                ?? throw new ArgumentNullException(nameof(identity));

            _router = router
                ?? throw new ArgumentNullException(nameof(router));

            _registration = registration
                ?? throw new ArgumentNullException(
                    nameof(registration));
        }

        public void Initialize()
        {
            _registration.Register(
                _identity.InstanceId,
                _router);
        }

        public void Dispose()
        {
            _registration.Unregister(
                _identity.InstanceId,
                _router);
        }
    }
}