using System;
using UnityEngine;

namespace Game.Actor
{
    public readonly struct ActorSpawnRequest
    {
        public ActorSpawnRequest(
            ActorInstance instance,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            Instance = instance
                ?? throw new ArgumentNullException(nameof(instance));

            Position = position;
            Rotation = rotation;
            Parent = parent;
        }

        public ActorInstance Instance { get; }

        public Vector3 Position { get; }

        public Quaternion Rotation { get; }

        public Transform Parent { get; }
    }
}