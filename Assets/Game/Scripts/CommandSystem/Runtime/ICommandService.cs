using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.CommandSystem
{
    public interface ICommandService
    {
        UniTask<CommandStatus> ExecuteAsync(
            ICommand command,
            CancellationToken cancellationToken);
    }
}