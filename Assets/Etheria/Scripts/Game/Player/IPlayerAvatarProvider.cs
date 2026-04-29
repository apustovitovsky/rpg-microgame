using System;

namespace Etheria.Game.Player
{
    public interface IPlayerAvatarProvider
    {
        PlayerAvatarContext? Current { get; }
        event Action<PlayerAvatarContext?> Changed;

        void Set(PlayerAvatarContext context);
        void Clear();
    }
}
