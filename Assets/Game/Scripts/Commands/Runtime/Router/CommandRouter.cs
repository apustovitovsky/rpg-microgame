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

        private readonly Dictionary<Type, ICommandExecutionEntry>
            _entries = new();

        private readonly Dictionary<ICommandExecutionGroup, ICommandScheduler>
            _schedulers = new();

        public CommandRouter(
            IInstanceIdentity identity,
            IEnumerable<ICommandExecutionEntry> entries)
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

            if (entries == null)
                return;

            foreach (var entry in entries)
            {
                if (entry == null)
                    continue;

                if (!_schedulers.ContainsKey(entry.ExecutionGroup))
                {
                    _schedulers.Add(
                        entry.ExecutionGroup,
                        new CommandScheduler(
                            entry.ExecutionGroup.ExecutionPolicy));
                }

                if (!_entries.TryAdd(
                        entry.CommandType,
                        entry))
                {
                    throw new InvalidOperationException(
                        $"Multiple command executions for command " +
                        $"'{entry.CommandType.Name}' are registered " +
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

            if (!_entries.TryGetValue(
                    command.GetType(),
                    out var entry) ||
                entry.ResultType != null)
            {
                return new CommandDispatchResult(
                    CommandDispatchStatus.Unsupported);
            }

            try
            {
                var scheduleResult = await ScheduleAsync(
                    entry,
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

            if (!_entries.TryGetValue(
                    command.GetType(),
                    out var entry) ||
                entry.ResultType != typeof(TResult))
            {
                return new CommandDispatchResult<TResult>(
                    CommandDispatchStatus.Unsupported,
                    default);
            }

            try
            {
                var scheduleResult = await ScheduleAsync(
                    entry,
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
                    $"Execution for '{entry.CommandType.Name}' returned " +
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
            _schedulers.Clear();
            _entries.Clear();
        }

        private UniTask<CommandScheduleResult> ScheduleAsync(
            ICommandExecutionEntry entry,
            object command,
            CancellationToken cancellationToken)
        {
            return _schedulers[entry.ExecutionGroup].ScheduleAsync(
                schedulerToken => entry.ExecuteAsync(
                    command,
                    new CommandContext(
                        _receiverId,
                        schedulerToken)),
                cancellationToken);
        }
    }
}