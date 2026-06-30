using System;

namespace Etheria.Game.World
{
    [Flags]
    public enum NavigationFlag
    {
        None = 0,

        Spawn = 1 << 0,
        Idle = 1 << 1,
        Dialogue = 1 << 2,

        Gate = 1 << 3,
        Tavern = 1 << 4,

        Indoor = 1 << 5,
        Outdoor = 1 << 6,

        Door = 1 << 7,
        GuardOnly = 1 << 8,
        Locked = 1 << 9,
        IndoorAccess = 1 << 10,
    }
}