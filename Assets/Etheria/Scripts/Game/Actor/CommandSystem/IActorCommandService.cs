using System.Threading;
using Cysharp.Threading.Tasks;

namespace Etheria.Game.Commands
{
    public interface IActorCommandService
    {
        UniTask<ActorCommandResult> ExecuteAsync(
            IActorCommand command,
            CancellationToken cancellationToken);
    }
}