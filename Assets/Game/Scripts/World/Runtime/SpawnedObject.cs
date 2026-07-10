using System;
using UnityEngine;

namespace Game.World
{
    public sealed class SpawnedObject : ISpawnedObject
    {
        private readonly CompositeLifetime _registrations = new();
        private bool _isDisposed;

        public SpawnedObject(
            IWorldInstance instance,
            GameObject gameObject)
        {
            Instance = instance
                ?? throw new ArgumentNullException(nameof(instance));

            if (Instance.InstanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "World instance id is required.",
                    nameof(instance));
            }

            GameObject = gameObject
                ?? throw new ArgumentNullException(nameof(gameObject));
        }

        public IWorldInstance Instance { get; }

        public Guid InstanceId => Instance.InstanceId;

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