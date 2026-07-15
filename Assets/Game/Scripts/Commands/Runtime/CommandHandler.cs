using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Commands
{
    public abstract class CommandHandler<TCommand> :
        ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public Type CommandType => typeof(TCommand);

        public UniTask<CommandResult> HandleAsync(
            ICommand command,
            Guid targetInstanceId,
            CancellationToken token)
        {
            if (command is not TCommand typedCommand)
            {
                return UniTask.FromResult(
                    CommandResult.Unsupported);
            }

            return HandleAsync(
                typedCommand,
                targetInstanceId,
                token);
        }

        public abstract UniTask<CommandResult> HandleAsync(
            TCommand command,
            Guid targetInstanceId,
            CancellationToken token);
    }
}