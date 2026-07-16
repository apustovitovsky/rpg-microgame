using System;

namespace Game.Commands
{
    public interface ICommandRouterRegistration
    {
        void Register(
            Guid instanceId,
            ICommandRouter router);

        bool Unregister(
            Guid instanceId,
            ICommandRouter expectedRouter);
    }
}