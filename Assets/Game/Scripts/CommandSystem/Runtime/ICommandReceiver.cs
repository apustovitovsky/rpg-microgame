using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.CommandSystem
{
    public interface ICommandReceiver
    {
        UniTask<CommandResult> ReceiveAsync(
            IWorldCommand command,
            CancellationToken token);
    }
}