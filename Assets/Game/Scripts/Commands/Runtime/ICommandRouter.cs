using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Commands
{
    public interface ICommandRouter
    {
        UniTask<CommandDispatchResult> RouteAsync(
            ICommand command,
            CancellationToken cancellationToken);

        UniTask<CommandDispatchResult<TResult>> RouteAsync<TResult>(
            ICommand<TResult> command,
            CancellationToken cancellationToken);
    }
}