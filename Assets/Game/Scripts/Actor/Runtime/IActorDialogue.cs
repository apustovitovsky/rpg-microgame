using System.Threading;
using Cysharp.Threading.Tasks;
using Game.CommandSystem;
using Game.World;

namespace Game.Actor
{
    public interface IActorDialogue
    {
        UniTask<CommandStatus> StartDialogueAsync(
            WorldId interactorWorldId,
            CancellationToken cancellationToken);
    }
}