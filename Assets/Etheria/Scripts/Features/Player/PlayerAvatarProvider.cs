using System;
using Etheria.Game.Actor;
using Etheria.Game.Player;

namespace Etheria.Features.Player
{
    public sealed class PlayerAvatarProvider : IPlayerAvatarProvider
    {
        public IPlayerAvatar Current { get; private set; }

        public event Action<IPlayerAvatar> Changed;

        public void Set(IPlayerAvatar avatar)
        {
            Current = avatar;
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
