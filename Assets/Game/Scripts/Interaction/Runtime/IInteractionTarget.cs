using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Interaction
{
    public interface IInteractionTarget
    {
        Vector3 InteractionPoint { get; }

        float MaxRange { get; }

        bool CanInteract(
            InteractionContext context);

        UniTask<InteractionResult> InteractAsync(
            InteractionContext context,
            CancellationToken token);
    }
}