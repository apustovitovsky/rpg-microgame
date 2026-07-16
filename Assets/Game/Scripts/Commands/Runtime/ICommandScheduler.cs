using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Commands
{
    internal interface ICommandScheduler :
        IDisposable
    {
        UniTask<CommandScheduleResult> ScheduleAsync(
            Func<
                CancellationToken,
                UniTask<CommandHandlerAdapterResult>> operation,
            CancellationToken cancellationToken);
    }

    internal readonly struct CommandScheduleResult
    {
        public CommandScheduleResult(
            bool wasDropped,
            CommandHandlerAdapterResult result)
        {
            WasDropped = wasDropped;
            Result = result;
        }

        public bool WasDropped { get; }

        public CommandHandlerAdapterResult Result { get; }

        public static CommandScheduleResult Dropped =>
            new(true, default);

        public static CommandScheduleResult Completed(
            CommandHandlerAdapterResult result)
        {
            return new CommandScheduleResult(
                false,
                result);
        }
    }
}