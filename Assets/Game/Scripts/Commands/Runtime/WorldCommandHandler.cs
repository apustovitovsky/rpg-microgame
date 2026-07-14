using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Commands
{
    public abstract class WorldCommandHandler<TCommand> :
        IWorldCommandHandler<TCommand>
        where TCommand : IWorldCommand
    {
        public Type CommandType => typeof(TCommand);

        public UniTask<CommandResult> HandleAsync(
            IWorldCommand command,
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