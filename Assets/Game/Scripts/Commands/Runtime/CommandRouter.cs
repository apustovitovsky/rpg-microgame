using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using UnityEngine;

namespace Game.Commands
{
    public sealed class CommandRouter :
        ICommandRouter,
        IDisposable
    {
        private readonly Guid _receiverId;

        private readonly Dictionary<Type, ICommandHandlerAdapter>
            _routes = new();

        private readonly Dictionary<ICommandRoutes, ICommandScheduler>
            _schedulers = new();

        public CommandRouter(
            IInstanceIdentity identity,
            IEnumerable<ICommandHandlerAdapter> routes)
        {
            if (identity == null)
                throw new ArgumentNullException(nameof(identity));

            _receiverId = identity.InstanceId;

            if (_receiverId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Instance id is required.",
                    nameof(identity));
            }

            if (routes == null)
                return;

            foreach (var route in routes)
            {
                if (route == null)
                    continue;

                if (!_schedulers.ContainsKey(route.Owner))
                {
                    _schedulers.Add(
                        route.Owner,
                        new CommandScheduler(
                            route.Owner.Ordering));
                }

                if (!_routes.TryAdd(
                        route.CommandType,
                        route))
                {
                    throw new InvalidOperationException(
                        $"Multiple handlers for command " +
                        $"'{route.CommandType.Name}' are registered " +
                        "on one router.");
                }
            }
        }

        public async UniTask<CommandDispatchResult> RouteAsync(
            ICommand command,
            CancellationToken cancellationToken)
        {
            if (command == null ||
                cancellationToken.IsCancellationRequested)
            {
                return new CommandDispatchResult(
                    cancellationToken.IsCancellationRequested
                        ? CommandDispatchStatus.Cancelled
                        : CommandDispatchStatus.Unsupported);
            }

            if (!_routes.TryGetValue(
                    command.GetType(),
                    out var route) ||
                route.ResultType != null)
            {
                return new CommandDispatchResult(
                    CommandDispatchStatus.Unsupported);
            }

            try
            {
                var scheduleResult = await ScheduleAsync(
                    route,
                    command,
                    cancellationToken);

                return new CommandDispatchResult(
                    scheduleResult.WasDropped
                        ? CommandDispatchStatus.Dropped
                        : CommandDispatchStatus.Delivered);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                return new CommandDispatchResult(
                    CommandDispatchStatus.Cancelled);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);

                return new CommandDispatchResult(
                    CommandDispatchStatus.Failed);
            }
        }

        public async UniTask<CommandDispatchResult<TResult>> RouteAsync<TResult>(
            ICommand<TResult> command,
            CancellationToken cancellationToken)
        {
            if ((object)command == null ||
                cancellationToken.IsCancellationRequested)
            {
                return new CommandDispatchResult<TResult>(
                    cancellationToken.IsCancellationRequested
                        ? CommandDispatchStatus.Cancelled
                        : CommandDispatchStatus.Unsupported,
                    default);
            }

            if (!_routes.TryGetValue(
                    command.GetType(),
                    out var route) ||
                route.ResultType != typeof(TResult))
            {
                return new CommandDispatchResult<TResult>(
                    CommandDispatchStatus.Unsupported,
                    default);
            }

            try
            {
                var scheduleResult = await ScheduleAsync(
                    route,
                    command,
                    cancellationToken);

                if (scheduleResult.WasDropped)
                {
                    return new CommandDispatchResult<TResult>(
                        CommandDispatchStatus.Dropped,
                        default);
                }

                var result = scheduleResult.Result.Value;

                if (result == null)
                {
                    return new CommandDispatchResult<TResult>(
                        CommandDispatchStatus.Delivered,
                        default);
                }

                if (result is TResult typedResult)
                {
                    return new CommandDispatchResult<TResult>(
                        CommandDispatchStatus.Delivered,
                        typedResult);
                }

                throw new InvalidOperationException(
                    $"Handler for '{route.CommandType.Name}' returned " +
                    $"'{result.GetType().Name}' instead of " +
                    $"'{typeof(TResult).Name}'.");
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                return new CommandDispatchResult<TResult>(
                    CommandDispatchStatus.Cancelled,
                    default);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);

                return new CommandDispatchResult<TResult>(
                    CommandDispatchStatus.Failed,
                    default);
            }
        }

        public void Dispose()
        {
            foreach (var scheduler in _schedulers.Values)
                scheduler.Dispose();

            _schedulers.Clear();
            _routes.Clear();
        }

        private UniTask<CommandScheduleResult> ScheduleAsync(
            ICommandHandlerAdapter route,
            object command,
            CancellationToken cancellationToken)
        {
            return _schedulers[route.Owner].ScheduleAsync(
                schedulerToken => route.RouteAsync(
                    command,
                    new CommandContext(
                        _receiverId,
                        schedulerToken)),
                cancellationToken);
        }
    }
}