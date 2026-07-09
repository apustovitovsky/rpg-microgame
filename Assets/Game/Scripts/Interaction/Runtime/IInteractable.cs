using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Interaction
{
    public interface IInteractable
    {
        Vector3 InteractionPoint { get; }
        float MaxRange { get; }
        bool CanInteract(InteractionContext context);
        UniTask InteractAsync(
            InteractionContext context,
            CancellationToken token);
    }
}