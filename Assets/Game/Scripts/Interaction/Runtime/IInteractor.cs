using UnityEngine;

namespace Game.Interaction
{
    public interface IInteractor
    {
        Vector3 InteractionOrigin { get; }
    }
}