using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Commands
{
    public interface ICommandReceiver
    {
        UniTask<CommandResult> ReceiveAsync(
            IWorldCommand command,
            CancellationToken token);
    }
}