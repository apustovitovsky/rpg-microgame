using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Actor
{
    public interface IActorDialogue
    {
        UniTask StartDialogueAsync(
            Guid interactorInstanceId,
            CancellationToken cancellationToken);
    }
}