using Cysharp.Threading.Tasks;

namespace Game.Commands
{
    public interface ICommandHandler<in TCommand>
        where TCommand : ICommand
    {
        UniTask HandleAsync(
            TCommand command,
            CommandContext context);
    }

    public interface ICommandHandler<in TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        UniTask<TResult> HandleAsync(
            TCommand command,
            CommandContext context);
    }
}