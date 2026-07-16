using System;
using Cysharp.Threading.Tasks;

namespace Game.Commands
{
    public interface ICommandHandlerAdapter
    {
        ICommandRoutes Owner { get; }

        Type CommandType { get; }

        Type ResultType { get; }

        UniTask<CommandHandlerAdapterResult> RouteAsync(
            object command,
            CommandContext context);
    }

    public readonly struct CommandHandlerAdapterResult
    {
        public CommandHandlerAdapterResult(
            object value)
        {
            Value = value;
        }

        public object Value { get; }
    }
}