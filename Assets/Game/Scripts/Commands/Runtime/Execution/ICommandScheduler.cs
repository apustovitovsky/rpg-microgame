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
                UniTask<CommandExecutionEntryResult>> operation,
            CancellationToken cancellationToken);
    }

    internal readonly struct CommandScheduleResult
    {
        public CommandScheduleResult(
            bool wasDropped,
            CommandExecutionEntryResult result)
        {
            WasDropped = wasDropped;
            Result = result;
        }

        public bool WasDropped { get; }

        public CommandExecutionEntryResult Result { get; }

        public static CommandScheduleResult Dropped =>
            new(true, default);

        public static CommandScheduleResult Completed(
            CommandExecutionEntryResult result)
        {
            return new CommandScheduleResult(
                false,
                result);
        }
    }
}