using Etheria.Game.Targeting;
using UnityEngine;

namespace Etheria.Game.Player
{
    public readonly struct PlayerAvatarContext
    {
        public PlayerAvatarContext(
            Transform root,
            Transform cameraPivot,
            IActorInputHandler inputHandler,
            ITargetable targetable)
        {
            Root = root;
            CameraPivot = cameraPivot;
            InputHandler = inputHandler;
            Targetable = targetable;
        }

        public Transform Root { get; }
        public Transform CameraPivot { get; }
        public IActorInputHandler InputHandler { get; }
        public ITargetable Targetable { get; }

        public bool IsValid =>
            Root != null &&
            CameraPivot != null &&
            InputHandler != null &&
            Targetable != null;
    }
}
