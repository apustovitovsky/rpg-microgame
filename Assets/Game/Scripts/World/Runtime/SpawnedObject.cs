using System;
using UnityEngine;

namespace Game.World
{
    public interface ISpawnedObject : IDisposable
    {
        Guid InstanceId { get; }

        GameObject GameObject { get; }

        bool IsDisposed { get; }

        void Add(IDisposable registration);
    }

    public sealed class SpawnedObject : ISpawnedObject
    {
        private readonly CompositeLifetime _registrations = new();
        private bool _isDisposed;

        public SpawnedObject(
            Guid instanceId,
            GameObject gameObject)
        {
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Instance id is required.",
                    nameof(instanceId));
            }

            InstanceId = instanceId;
            GameObject = gameObject
                ?? throw new ArgumentNullException(nameof(gameObject));
        }

        public Guid InstanceId { get; }

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