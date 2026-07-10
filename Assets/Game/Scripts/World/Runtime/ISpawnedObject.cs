using System;
using UnityEngine;

namespace Game.World
{
    public interface ISpawnedObject : IDisposable
    {
        IWorldInstance Instance { get; }

        Guid InstanceId { get; }

        GameObject GameObject { get; }

        bool IsDisposed { get; }

        void Add(IDisposable registration);
    }
}