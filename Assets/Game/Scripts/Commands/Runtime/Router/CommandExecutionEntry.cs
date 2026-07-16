using System;
using Cysharp.Threading.Tasks;

namespace Game.Commands
{
    internal sealed class CommandExecutionEntry<
        TExecutionGroup,
        TCommand> :
        ICommandExecutionEntry
        where TExecutionGroup : class,
            ICommandExecutionGroup,
            ICommandExecution<TCommand>
        where TCommand : ICommand
    {
        private readonly TExecutionGroup _executionGroup;

        public CommandExecutionEntry(
            TExecutionGroup executionGroup)
        {
            _executionGroup = executionGroup
                ?? throw new ArgumentNullException(nameof(executionGroup));
        }

        public ICommandExecutionGroup ExecutionGroup => _executionGroup;

        public Type CommandType => typeof(TCommand);

        public Type ResultType => null;

        public async UniTask<CommandExecutionEntryResult> ExecuteAsync(
            object command,
            CommandContext context)
        {
            if (command is not TCommand typedCommand)
            {
                throw new ArgumentException(
                    $"Expected command '{typeof(TCommand).Name}'.",
                    nameof(command));
            }

            await _executionGroup.ExecuteAsync(
                typedCommand,
                context);

            return default;
        }
    }

    internal sealed class CommandExecutionAdapter<
        TExecutionGroup,
        TCommand,
        TResult> :
        ICommandExecutionEntry
        where TExecutionGroup : class,
            ICommandExecutionGroup,
            ICommandExecution<TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        private readonly TExecutionGroup _executionGroup;

        public CommandExecutionAdapter(
            TExecutionGroup executionGroup)
        {
            _executionGroup = executionGroup
                ?? throw new ArgumentNullException(nameof(executionGroup));
        }

        public ICommandExecutionGroup ExecutionGroup => _executionGroup;

        public Type CommandType => typeof(TCommand);

        public Type ResultType => typeof(TResult);

        public async UniTask<CommandExecutionEntryResult> ExecuteAsync(
            object command,
            CommandContext context)
        {
            if (command is not TCommand typedCommand)
            {
                throw new ArgumentException(
                    $"Expected command '{typeof(TCommand).Name}'.",
                    nameof(command));
            }

            var result = await _executionGroup.ExecuteAsync(
                typedCommand,
                context);

            return new CommandExecutionEntryResult(result);
        }
    }
}