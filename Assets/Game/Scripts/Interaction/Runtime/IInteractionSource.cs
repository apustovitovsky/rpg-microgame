using UnityEngine;

namespace Game.Interaction
{
    public interface IInteractionSource
    {
        Vector3 InteractionOrigin { get; }
        float MaxRange { get; }
    }
}