using UnityEngine;

namespace Etheria.Game.Player
{
    public interface IActorFacingHandler
    {
        void HandleFace(Vector3 direction);
    }
}
