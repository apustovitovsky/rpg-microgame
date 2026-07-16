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

                _ => throw new ArgumentOutOfRangeException(
                    nameof(_policy),
                    _policy,
                    "Unknown command execution policy.")
            };
        }

        public void Dispose()
        {
            _gate.Dispose();
        }

        private static async UniTask<CommandScheduleResult> RunConcurrentAsync(
            Func<
                CancellationToken,
                UniTask<CommandExecutionEntryResult>> operation,
            CancellationToken cancellationToken)
        {
            return CommandScheduleResult.Completed(
                await operation(cancellationToken));
        }

        private async UniTask<CommandScheduleResult> RunOrDropAsync(
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

        private async UniTask<CommandScheduleResult> RunSequentialAsync(
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
    }
}