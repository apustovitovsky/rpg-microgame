using System;
using Etheria.Game.Common;

namespace Etheria.Game.Actor
{
    public interface IPlayerAvatarInfo : IIdentifiable<Guid>
    {
        string DisplayName { get; }
    }
}