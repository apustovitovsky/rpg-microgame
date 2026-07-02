using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.CommandSystem
{
    public abstract class CommandHandler<TCommand> :
        ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public Type CommandType => typeof(TCommand);

        public UniTask<CommandStatus> HandleAsync(
            ICommand command,
            CancellationToken cancellationToken)
        {
            if (command is not TCommand typedCommand)
            {
                return UniTask.FromResult(
                    CommandStatus.InvalidCommand);
            }

            return HandleAsync(
                typedCommand,
                cancellationToken);
        }

        public abstract UniTask<CommandStatus> HandleAsync(
            TCommand command,
            CancellationToken cancellationToken);
    }
}