using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Commands
{
    public sealed class CommandBus :
        ICommandBus,
        ICommandRouterRegistrar
    {
        private readonly Dictionary<Guid, ICommandRouter> _routers =
            new();

        public void Register(
            Guid instanceId,
            ICommandRouter router)
        {
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Instance id is required.",
                    nameof(instanceId));
            }

            if (router == null)
                throw new ArgumentNullException(nameof(router));

            if (!_routers.TryAdd(instanceId, router))
            {
                throw new InvalidOperationException(
                    $"Command router is already registered for " +
                    $"'{instanceId}'.");
            }
        }

        public bool Unregister(
            Guid instanceId,
            ICommandRouter expectedRouter)
        {
            if (instanceId == Guid.Empty ||
                expectedRouter == null ||
                !_routers.TryGetValue(
                    instanceId,
                    out var router) ||
                !ReferenceEquals(router, expectedRouter))
            {
                return false;
            }

            return _routers.Remove(instanceId);
        }

        public UniTask<CommandDispatchResult> SendAsync(
            Guid targetInstanceId,
            ICommand command,
            CancellationToken cancellationToken)
        {
            if (targetInstanceId == Guid.Empty ||
                !_routers.TryGetValue(
                    targetInstanceId,
                    out var router))
            {
                return UniTask.FromResult(
                    new CommandDispatchResult(
                        CommandDispatchStatus.TargetNotFound));
            }

            return router.RouteAsync(
                command,
                cancellationToken);
        }

        public UniTask<CommandDispatchResult<TResult>> RequestAsync<TResult>(
            Guid targetInstanceId,
            ICommand<TResult> command,
            CancellationToken cancellationToken)
        {
            if (targetInstanceId == Guid.Empty ||
                !_routers.TryGetValue(
                    targetInstanceId,
                    out var router))
            {
                return UniTask.FromResult(
                    new CommandDispatchResult<TResult>(
                        CommandDispatchStatus.TargetNotFound,
                        default));
            }

            return router.RouteAsync(
                command,
                cancellationToken);
        }

        public async UniTask<TResult> RequestRequiredAsync<TResult>(
            Guid targetInstanceId,
            ICommand<TResult> command,
            CancellationToken cancellationToken)
        {
            var result = await RequestAsync(
                targetInstanceId,
                command,
                cancellationToken);

            if (result.IsDelivered)
                return result.Value;

            throw new InvalidOperationException(
                $"Command request to '{targetInstanceId}' failed: " +
                $"{result.Status}.");
        }
    }
}