using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Actor
{
    public sealed class ActorRegistry :
        IActorRegistry,
        IActorRegistryWriter
    {
        private readonly Dictionary<string, ActorInstance> _actors =
            new(StringComparer.Ordinal);

        public bool TryGet(
            string actorId,
            out ActorInstance actor)
        {
            actor = null;

            if (string.IsNullOrWhiteSpace(actorId))
                return false;

            if (!_actors.TryGetValue(actorId, out actor))
                return false;

            if (IsAlive(actor))
                return true;

            _actors.Remove(actorId);
            actor = null;
            return false;
        }

        public void Register(ActorInstance actor)
        {
            if (actor == null ||
                string.IsNullOrWhiteSpace(actor.InstanceId))
            {
                return;
            }

            if (_actors.TryGetValue(actor.InstanceId, out var existing) &&
                !ReferenceEquals(existing, actor))
            {
                Debug.LogWarning(
                    $"Actor instance id '{actor.InstanceId}' is already registered. New registration will replace previous one.");
            }

            _actors[actor.InstanceId] = actor;
        }

        public void Unregister(ActorInstance actor)
        {
            if (actor == null ||
                string.IsNullOrWhiteSpace(actor.InstanceId))
            {
                return;
            }

            if (_actors.TryGetValue(actor.InstanceId, out var existing) &&
                ReferenceEquals(existing, actor))
            {
                _actors.Remove(actor.InstanceId);
            }
        }

        private static bool IsAlive(ActorInstance actor)
        {
            if (actor?.View == null)
                return false;

            if (actor.View is UnityEngine.Object unityObject)
                return unityObject != null;

            return true;
        }
    }
}