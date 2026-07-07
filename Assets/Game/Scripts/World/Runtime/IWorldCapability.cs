using System;
using System.Collections.Generic;

namespace Game.World
{
    public interface IWorldCapability
    {
        IEnumerable<Type> PublishedTypes { get; }
    }
}