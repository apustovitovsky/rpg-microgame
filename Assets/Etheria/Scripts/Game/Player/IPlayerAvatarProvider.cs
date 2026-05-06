using System;
using Etheria.Game.Actor;

namespace Etheria.Game.Player
{
    public interface IPlayerAvatarProvider
    {
        IPlayerAvatar Current { get; }
        event Action<IPlayerAvatar> Changed;

        void Set(IPlayerAvatar avatar);
        void Clear();
    }
}
