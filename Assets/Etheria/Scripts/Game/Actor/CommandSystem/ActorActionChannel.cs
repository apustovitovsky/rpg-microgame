using System;

namespace Etheria.Game.Commands
{
    [Flags]
    public enum ActorActionChannel
    {
        None = 0,
        Locomotion = 1 << 0,
        Combat = 1 << 1,
        Interaction = 1 << 2,
        Dialogue = 1 << 3,
        Inventory = 1 << 4,
        Animation = 1 << 5,
        Scripted = 1 << 6,

        All = Locomotion |
              Combat |
              Interaction |
              Dialogue |
              Inventory |
              Animation |
              Scripted
    }
}