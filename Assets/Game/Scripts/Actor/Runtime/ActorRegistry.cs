using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Actor
{
    public sealed class ActorRegistry :
        IActorRegistry,
        IActorRegistryWriter
    {
        private readonly Dictionary<string, IActorView> _actors =
            new(StringComparer.Ordinal);

        public bool TryGet(
            string actorId,
            out IActorView actor)
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

        public void Register(IActorView actor)
        {
            if (actor == null ||
                string.IsNullOrWhiteSpace(actor.ActorId))
            {
                return;
            }

            if (_actors.TryGetValue(actor.ActorId, out var existing) &&
                !ReferenceEquals(existing, actor))
            {
                Debug.LogWarning(
                    $"Actor id '{actor.ActorId}' is already registered. New registration will replace previous one.");
            }

            _actors[actor.ActorId] = actor;
        }

        public void Unregister(IActorView actor)
        {
            if (actor == null ||
                string.IsNullOrWhiteSpace(actor.ActorId))
            {
                return;
            }

            if (_actors.TryGetValue(actor.ActorId, out var existing) &&
                ReferenceEquals(existing, actor))
            {
                _actors.Remove(actor.ActorId);
            }
        }

        private static bool IsAlive(IActorView actor)
        {
            if (actor == null)
                return false;

            if (actor is UnityEngine.Object unityObject)
                return unityObject != null;

            return true;
        }
    }
}