using System;
using Etheria.Game.Player;

namespace Etheria.Features.Player
{
    public sealed class PlayerAvatarProvider : IPlayerAvatarProvider
    {
        public PlayerAvatarContext? Current { get; private set; }

        public event Action<PlayerAvatarContext?> Changed;

        public void Set(PlayerAvatarContext context)
        {
            Current = context;
            Changed?.Invoke(Current);
        }

        public void Clear()
        {
            if (Current == null)
                return;

            Current = null;
            Changed?.Invoke(null);
        }
    }
}
