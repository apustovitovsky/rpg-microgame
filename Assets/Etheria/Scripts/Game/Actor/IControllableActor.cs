using Etheria.Game.Player;
using Etheria.Game.Targeting;
using UnityEngine;

namespace Etheria.Game.Actor
{
    public interface IControllableActor
    {
        Transform Root { get; }
        Transform CameraPivot { get; }
        PlayerAvatarHandlers Handlers { get; }
        ITargetable Targetable { get; }
    }
}
