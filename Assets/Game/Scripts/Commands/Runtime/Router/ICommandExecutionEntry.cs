using System;
using Cysharp.Threading.Tasks;

namespace Game.Commands
{
    public interface ICommandExecutionEntry
    {
        ICommandExecutionGroup ExecutionGroup { get; }

        Type CommandType { get; }

        Type ResultType { get; }

        UniTask<CommandExecutionEntryResult> ExecuteAsync(
            object command,
            CommandContext context);
    }

    public readonly struct CommandExecutionEntryResult
    {
        public CommandExecutionEntryResult(
            object value)
        {
            Value = value;
        }

        public object Value { get; }
    }
}