using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Commands
{
    internal sealed class CommandScheduler :
        ICommandScheduler
    {
        private readonly CommandOrdering _ordering;
        private readonly SemaphoreSlim _gate = new(1, 1);

        public CommandScheduler(
            CommandOrdering ordering)
        {
            _ordering = ordering;
        }

        public UniTask<CommandScheduleResult> ScheduleAsync(
            Func<
                CancellationToken,
                UniTask<CommandHandlerAdapterResult>> operation,
            CancellationToken cancellationToken)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            return _ordering switch
            {
                CommandOrdering.Parallel =>
                    RunParallelAsync(
                        operation,
                        cancellationToken),

                CommandOrdering.Drop =>
                    RunOrDropAsync(
                        operation,
                        cancellationToken),

                CommandOrdering.Sequential =>
                    RunSequentialAsync(
                        operation,
                        cancellationToken),

                _ => throw new ArgumentOutOfRangeException(
                    nameof(_ordering),
                    _ordering,
                    "Unknown command ordering.")
            };
        }

        public void Dispose()
        {
            _gate.Dispose();
        }

        private static async UniTask<CommandScheduleResult> RunParallelAsync(
            Func<
                CancellationToken,
                UniTask<CommandHandlerAdapterResult>> operation,
            CancellationToken cancellationToken)
        {
            return CommandScheduleResult.Completed(
                await operation(cancellationToken));
        }

        private async UniTask<CommandScheduleResult> RunOrDropAsync(
            Func<
                CancellationToken,
                UniTask<CommandHandlerAdapterResult>> operation,
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
                UniTask<CommandHandlerAdapterResult>> operation,
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