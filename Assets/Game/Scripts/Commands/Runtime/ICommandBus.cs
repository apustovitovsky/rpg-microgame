using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Commands
{
    public interface ICommandBus
    {
        UniTask<CommandDispatchResult> SendAsync(
            Guid targetInstanceId,
            ICommand command,
            CancellationToken cancellationToken);

        UniTask<CommandDispatchResult<TResult>> RequestAsync<TResult>(
            Guid targetInstanceId,
            ICommand<TResult> command,
            CancellationToken cancellationToken);

        UniTask<TResult> RequestRequiredAsync<TResult>(
            Guid targetInstanceId,
            ICommand<TResult> command,
            CancellationToken cancellationToken);
    }
}