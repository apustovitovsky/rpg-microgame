using System;

namespace Game.World
{
    public interface IWorldInstance
    {
        Guid InstanceId { get; }

        string DisplayName { get; }
    }
}