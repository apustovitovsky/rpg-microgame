using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.CommandSystem
{
    public interface ICommandHandler
    {
        Type CommandType { get; }

        UniTask<CommandStatus> HandleAsync(
            ICommand command,
            CancellationToken cancellationToken);
    }

    public interface ICommandHandler<TCommand> : ICommandHandler
        where TCommand : ICommand
    {
        UniTask<CommandStatus> HandleAsync(
            TCommand command,
            CancellationToken cancellationToken);
    }
}