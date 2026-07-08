using System;
using UnityEngine;

namespace Game.World
{
    public interface IWorldLifetime : IDisposable
    {
        WorldId WorldId { get; }

        GameObject GameObject { get; }

        bool IsDisposed { get; }

        void Add(IRegistrationToken registration);
    }

    public sealed class WorldLifetime : IWorldLifetime
    {
        private readonly CompositeRegistration _registrations = new();
        private bool _isDisposed;

        public WorldLifetime(
            WorldId worldId,
            GameObject gameObject)
        {
            WorldId = worldId;
            GameObject = gameObject;
        }

        public WorldId WorldId { get; }

        public GameObject GameObject { get; }

        public bool IsDisposed => _isDisposed;

        public void Add(IRegistrationToken registration)
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