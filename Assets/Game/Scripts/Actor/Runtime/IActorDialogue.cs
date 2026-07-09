using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Actor
{
    public interface IActorDialogue
    {
        UniTask StartDialogueAsync(
            WorldId interactorWorldId,
            CancellationToken cancellationToken);
    }
}