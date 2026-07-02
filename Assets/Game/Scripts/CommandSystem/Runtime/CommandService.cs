using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.CommandSystem
{
    public sealed class CommandService : ICommandService
    {
        private readonly Dictionary<Type, ICommandHandler> _handlers =
            new();

        public CommandService(
            IEnumerable<ICommandHandler> handlers)
        {
            if (handlers == null)
            {
                return;
            }

            foreach (var handler in handlers)
            {
                if (handler == null ||
                    handler.CommandType == null)
                {
                    continue;
                }

                _handlers[handler.CommandType] = handler;
            }
        }

        public UniTask<CommandStatus> ExecuteAsync(
            ICommand command,
            CancellationToken cancellationToken)
        {
            if (command == null)
            {
                return UniTask.FromResult(
                    CommandStatus.InvalidCommand);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return UniTask.FromResult(
                    CommandStatus.Cancelled);
            }

            return _handlers.TryGetValue(
                    command.GetType(),
                    out var handler)
                ? handler.HandleAsync(command, cancellationToken)
                : UniTask.FromResult(CommandStatus.HandlerNotFound);
        }
    }
}