using System.Threading;
using Cysharp.Threading.Tasks;
using Game.CommandSystem;

namespace Game.Actor
{
    public interface IActorDialogueHandler
    {
        UniTask<CommandStatus> StartDialogueAsync(
            string targetActorId,
            CancellationToken cancellationToken);
    }
}