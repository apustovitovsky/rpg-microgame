using Etheria.Game.Actor;
using UnityEngine;

namespace Etheria.Game.Player
{
    public interface IPlayerAvatar : IActorCapabilityProvider
    {
        IPlayerAvatarInfo Info { get; }
        Transform Root { get; }
        Transform CameraPivot { get; }
        IActorInputHandler InputHandler { get; }
        IActorFacingHandler FacingHandler { get; }
    }
}
