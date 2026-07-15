using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Commands
{
    public interface ICommandHandler
    {
        Type CommandType { get; }

        UniTask<CommandResult> HandleAsync(
            ICommand command,
            Guid targetInstanceId,
            CancellationToken token);
    }

    public interface ICommandHandler<TCommand> :
        ICommandHandler
        where TCommand : ICommand
    {
        UniTask<CommandResult> HandleAsync(
            TCommand command,
            Guid targetInstanceId,
            CancellationToken token);
    }
}