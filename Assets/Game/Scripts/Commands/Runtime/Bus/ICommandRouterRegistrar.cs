using System;

namespace Game.Commands
{
    public interface ICommandRouterRegistrar
    {
        void Register(
            Guid instanceId,
            ICommandRouter router);

        bool Unregister(
            Guid instanceId,
            ICommandRouter expectedRouter);
    }
}