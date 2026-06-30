using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Game.World;

namespace Etheria.Game.Commands
{
    public interface IActorCommandEndpoint
    {
        UniTask<ActorCommandResult> StartDialogueAsync(
            string targetActorId,
            CancellationToken cancellationToken);

        UniTask<ActorCommandResult> MoveToLocationAsync(
            string locationId,
            string anchorKey,
            NavigationQueryFilter filter,
            CancellationToken cancellationToken);
    }
}