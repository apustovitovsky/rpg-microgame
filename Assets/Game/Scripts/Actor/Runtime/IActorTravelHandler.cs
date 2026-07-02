using System.Threading;
using Cysharp.Threading.Tasks;
using Game.CommandSystem;

namespace Game.Actor
{
    public interface IActorTravelHandler
    {
        UniTask<CommandStatus> MoveToLocationAsync(
            string locationId,
            string anchorKey,
            CancellationToken cancellationToken);
    }
}