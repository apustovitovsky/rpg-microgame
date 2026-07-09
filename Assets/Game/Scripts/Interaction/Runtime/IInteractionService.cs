using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;
using UnityEngine;

namespace Game.Interaction
{
    public interface IInteractionService
    {
        UniTask<InteractionResult> TryInteractAsync(
            WorldId interactorWorldId,
            Vector3 interactionOrigin,
            WorldId targetWorldId,
            CancellationToken token);
    }
}