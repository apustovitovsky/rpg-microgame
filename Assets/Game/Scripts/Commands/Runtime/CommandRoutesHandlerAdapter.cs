using System;
using Cysharp.Threading.Tasks;

namespace Game.Commands
{
    internal sealed class CommandRoutesHandlerAdapter<
        TRoutes,
        TCommand> :
        ICommandHandlerAdapter
        where TRoutes : class,
            ICommandRoutes,
            ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly TRoutes _routes;

        public CommandRoutesHandlerAdapter(
            TRoutes routes)
        {
            _routes = routes
                ?? throw new ArgumentNullException(nameof(routes));
        }

        public ICommandRoutes Owner => _routes;

        public Type CommandType => typeof(TCommand);

        public Type ResultType => null;

        public async UniTask<CommandHandlerAdapterResult> RouteAsync(
            object command,
            CommandContext context)
        {
            if (command is not TCommand typedCommand)
            {
                throw new ArgumentException(
                    $"Expected command '{typeof(TCommand).Name}'.",
                    nameof(command));
            }

            await _routes.HandleAsync(
                typedCommand,
                context);

            return default;
        }
    }

    internal sealed class CommandRoutesHandlerAdapter<
        TRoutes,
        TCommand,
        TResult> :
        ICommandHandlerAdapter
        where TRoutes : class,
            ICommandRoutes,
            ICommandHandler<TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        private readonly TRoutes _routes;

        public CommandRoutesHandlerAdapter(
            TRoutes routes)
        {
            _routes = routes
                ?? throw new ArgumentNullException(nameof(routes));
        }

        public ICommandRoutes Owner => _routes;

        public Type CommandType => typeof(TCommand);

        public Type ResultType => typeof(TResult);

        public async UniTask<CommandHandlerAdapterResult> RouteAsync(
            object command,
            CommandContext context)
        {
            if (command is not TCommand typedCommand)
            {
                throw new ArgumentException(
                    $"Expected command '{typeof(TCommand).Name}'.",
                    nameof(command));
            }

            var result = await _routes.HandleAsync(
                typedCommand,
                context);

            return new CommandHandlerAdapterResult(result);
        }
    }
}