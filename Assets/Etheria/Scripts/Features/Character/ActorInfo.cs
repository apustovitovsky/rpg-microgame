using System;
using Etheria.Game.Actor;
using Etheria.Game.Common;

namespace Etheria.Features.Actor
{
    public sealed class ActorInfo : IPlayerAvatarInfo
    {
        public ActorInfo(IPrefixedStringGenerator nameGenerator)
        {
            Id = Guid.NewGuid();
            DisplayName = nameGenerator.Generate("Actor");
        }

        public Guid Id { get; }

        public string DisplayName { get; }
    }
}
