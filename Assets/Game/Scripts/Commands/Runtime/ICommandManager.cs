using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Commands
{
    public interface ICommandManager
    {
        UniTask<CommandResult> SendAsync(
            Guid targetInstanceId,
            ICommand command,
            CancellationToken token);
    }
}