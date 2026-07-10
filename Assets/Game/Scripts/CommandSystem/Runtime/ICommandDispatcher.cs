using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.CommandSystem
{
    public interface ICommandDispatcher
    {
        UniTask<CommandResult> SendAsync(
            Guid targetInstanceId,
            IWorldCommand command,
            CancellationToken token);
    }
}