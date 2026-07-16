using Cysharp.Threading.Tasks;

namespace Game.Commands
{
    public interface ICommandExecution<in TCommand>
        where TCommand : ICommand
    {
        UniTask ExecuteAsync(
            TCommand command,
            CommandContext context);
    }

    public interface ICommandExecution<in TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        UniTask<TResult> ExecuteAsync(
            TCommand command,
            CommandContext context);
    }
}