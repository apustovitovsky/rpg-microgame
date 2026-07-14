using System;
using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.World
{
    public sealed class WorldSpawner :
        IWorldSpawner
    {
        private readonly LifetimeScope _parentScope;
        private readonly Registry<GameObject> _roots = new();

        public WorldSpawner(LifetimeScope parentScope)
        {
            _parentScope = parentScope != null
                ? parentScope
                : throw new ArgumentNullException(nameof(parentScope));
        }

        public GameObject Spawn(
            Guid instanceId,
            GameObject prefab,
            SpawnPlacement placement,
            IInstaller installer)
        {
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Instance id is required.",
                    nameof(instanceId));
            }

            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            if (installer == null)
                throw new ArgumentNullException(nameof(installer));

            if (_roots.Contains(instanceId))
            {
                throw new InvalidOperationException(
                    $"World object '{instanceId}' is already spawned.");
            }

            GameObject root = null;

            try
            {
                using (LifetimeScope.EnqueueParent(_parentScope))
                using (LifetimeScope.Enqueue(installer))
                {
                    root = UnityEngine.Object.Instantiate(
                        prefab,
                        placement.Position,
                        placement.Rotation,
                        placement.Parent);
                }

                _roots.Add(instanceId, root);

                return root;
            }
            catch
            {
                if (root != null)
                    UnityEngine.Object.Destroy(root);

                throw;
            }
        }

        public bool Despawn(Guid instanceId)
        {
            if (!_roots.TryGet(
                    instanceId,
                    out var root))
            {
                return false;
            }

            if (!_roots.Remove(
                    instanceId,
                    root))
            {
                return false;
            }

            UnityEngine.Object.Destroy(root);

            return true;
        }
    }
}