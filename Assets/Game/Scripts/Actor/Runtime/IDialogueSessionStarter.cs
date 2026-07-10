using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Actor
{
    public interface IDialogueSessionStarter
    {
        UniTask StartDialogueAsync(
            Guid interactorInstanceId,
            CancellationToken cancellationToken);
    }
}