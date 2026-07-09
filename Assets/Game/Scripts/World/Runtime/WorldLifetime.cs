using System;
using UnityEngine;

namespace Game.World
{
    public interface IWorldLifetime : IDisposable
    {
        WorldId WorldId { get; }

        WorldInfo Info { get; }

        GameObject GameObject { get; }

        bool IsDisposed { get; }

        void Add(IDisposable registration);
    }

    public sealed class WorldLifetime : IWorldLifetime
    {
        private readonly CompositeLifetime _registrations = new();
        private bool _isDisposed;

        public WorldLifetime(
            GameObject gameObject,
            WorldInfo info)
        {
            GameObject = gameObject;
            Info = info;
        }

        public WorldId WorldId => Info.WorldId;

        public WorldInfo Info { get; }

        public GameObject GameObject { get; }

        public bool IsDisposed => _isDisposed;

        public void Add(IDisposable registration)
        {
            if (registration == null)
                return;

            if (_isDisposed)
            {
                registration.Dispose();
                return;
            }

            _registrations.Add(registration);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _registrations.Dispose();

            if (GameObject != null)
                UnityEngine.Object.Destroy(GameObject);
        }
    }
}