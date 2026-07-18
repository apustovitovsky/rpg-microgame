using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Commands
{
    internal sealed class CommandScheduler :
        ICommandScheduler
    {
        private readonly CommandExecutionPolicy _policy;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private readonly object _switchLock = new();

        private CancellationTokenSource _switchCancellation;

        public CommandScheduler(
            CommandExecutionPolicy policy)
        {
            _policy = policy;
        }

        public UniTask<CommandScheduleResult> ScheduleAsync(
            Func<
                CancellationToken,
                UniTask<CommandExecutionEntryResult>> operation,
            CancellationToken cancellationToken)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            return _policy switch
            {
                CommandExecutionPolicy.Concurrent =>
                    RunConcurrentAsync(
                        operation,
                        cancellationToken),

                CommandExecutionPolicy.Drop =>
                    RunOrDropAsync(
                        operation,
                        cancellationToken),

                CommandExecutionPolicy.Sequential =>
                    RunSequentialAsync(
                        operation,
                        cancellationToken),

                CommandExecutionPolicy.Switch =>
                    RunSwitchAsync(
                        operation,
                        cancellationToken),

                _ => throw new ArgumentOutOfRangeException(
                    nameof(_policy),
                    _policy,
                    "Unknown command execution policy.")
            };
        }

        private static async UniTask<CommandScheduleResult>
            RunConcurrentAsync(
                Func<
                    CancellationToken,
                    UniTask<CommandExecutionEntryResult>> operation,
                CancellationToken cancellationToken)
        {
            return CommandScheduleResult.Completed(
                await operation(cancellationToken));
        }

        private async UniTask<CommandScheduleResult>
            RunOrDropAsync(
                Func<
                    CancellationToken,
                    UniTask<CommandExecutionEntryResult>> operation,
                CancellationToken cancellationToken)
        {
            if (!_gate.Wait(0))
                return CommandScheduleResult.Dropped;

            try
            {
                return CommandScheduleResult.Completed(
                    await operation(cancellationToken));
            }
            finally
            {
                _gate.Release();
            }
        }

        private async UniTask<CommandScheduleResult>
            RunSequentialAsync(
                Func<
                    CancellationToken,
                    UniTask<CommandExecutionEntryResult>> operation,
                CancellationToken cancellationToken)
        {
            await _gate.WaitAsync(cancellationToken);

            try
            {
                return CommandScheduleResult.Completed(
                    await operation(cancellationToken));
            }
            finally
            {
                _gate.Release();
            }
        }

        private async UniTask<CommandScheduleResult>
            RunSwitchAsync(
                Func<
                    CancellationToken,
                    UniTask<CommandExecutionEntryResult>> operation,
                CancellationToken cancellationToken)
        {
            var switchCancellation =
                ReplaceSwitchCancellation(
                    cancellationToken);

            try
            {
                return CommandScheduleResult.Completed(
                    await operation(
                        switchCancellation.Token));
            }
            catch (OperationCanceledException)
                when (switchCancellation.IsCancellationRequested &&
                      !cancellationToken.IsCancellationRequested)
            {
                return CommandScheduleResult.Dropped;
            }
            finally
            {
                ClearSwitchCancellation(
                    switchCancellation);

                switchCancellation.Dispose();
            }
        }

        private CancellationTokenSource
            ReplaceSwitchCancellation(
                CancellationToken cancellationToken)
        {
            lock (_switchLock)
            {
                _switchCancellation?.Cancel();

                _switchCancellation =
                    CancellationTokenSource
                        .CreateLinkedTokenSource(
                            cancellationToken);

                return _switchCancellation;
            }
        }

        private void ClearSwitchCancellation(
            CancellationTokenSource switchCancellation)
        {
            lock (_switchLock)
            {
                if (ReferenceEquals(
                        _switchCancellation,
                        switchCancellation))
                {
                    _switchCancellation = null;
                }
            }
        }
    }
}