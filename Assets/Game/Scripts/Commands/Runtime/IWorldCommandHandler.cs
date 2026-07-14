using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Commands
{
    public interface IWorldCommandHandler
    {
        Type CommandType { get; }

        UniTask<CommandResult> HandleAsync(
            IWorldCommand command,
            Guid targetInstanceId,
            CancellationToken token);
    }

    public interface IWorldCommandHandler<TCommand> :
        IWorldCommandHandler
        where TCommand : IWorldCommand
    {
        UniTask<CommandResult> HandleAsync(
            TCommand command,
            Guid targetInstanceId,
            CancellationToken token);
    }
}