using Etheria.Game.Targeting;
using UnityEngine;

namespace Etheria.Game.Player
{
    public readonly struct PlayerAvatarContext
    {
        public PlayerAvatarContext(
            Transform root,
            Transform cameraPivot,
            PlayerAvatarHandlers handlers,
            ITargetable targetable)
        {
            Root = root;
            CameraPivot = cameraPivot;
            Handlers = handlers;
            Targetable = targetable;
        }

        public Transform Root { get; }
        public Transform CameraPivot { get; }
        public PlayerAvatarHandlers Handlers { get; }
        public ITargetable Targetable { get; }

        public bool IsValid =>
            Root != null &&
            CameraPivot != null &&
            Handlers.IsValid &&
            Targetable != null;
    }
}
