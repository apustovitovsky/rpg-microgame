using System;
using UnityEngine;
using VContainer.Unity;

namespace Game.Actor
{
    public sealed class ActorSpawner : IActorSpawner
    {
        private readonly LifetimeScope _parentScope;

        public ActorSpawner(LifetimeScope parentScope)
        {
            _parentScope = parentScope;
        }

        public IActorView Spawn(
            string actorId,
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            actorId = actorId?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(actorId))
                throw new ArgumentException("Actor id is required.", nameof(actorId));

            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            using (LifetimeScope.EnqueueParent(_parentScope))
            {
                var instance = UnityEngine.Object.Instantiate(
                    prefab,
                    position,
                    rotation,
                    parent);

                var view = instance.GetComponentInChildren<ActorView>(true);

                if (view == null)
                {
                    throw new InvalidOperationException(
                        $"Actor prefab '{prefab.name}' has no {nameof(ActorView)}.");
                }

                view.Initialize(actorId);

                return view;
            }
        }
    }
}