using System;
using Etheria.Core.DI;
using Etheria.Game.Npc;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Npc
{
    public sealed class NpcSpawner : INpcSpawner
    {
        private readonly NpcCatalogSO _catalog;
        private readonly ScopeRoot _scopeContentRoot;
        private readonly LifetimeScope _parentScope;

        public NpcSpawner(
            NpcCatalogSO catalog,
            ScopeRoot scopeContentRoot,
            LifetimeScope parentScope)
        {
            _catalog = catalog;
            _scopeContentRoot = scopeContentRoot;
            _parentScope = parentScope;
        }

        public GameObject Spawn(
            string npcId,
            Vector3 position,
            Quaternion rotation)
        {
            if (!_catalog.TryGet(npcId, out var definition))
            {
                throw new InvalidOperationException(
                    $"Character definition '{npcId}' was not found.");
            }

            if (definition.Prefab == null)
            {
                throw new InvalidOperationException(
                    $"Character '{npcId}' has no prefab.");
            }

            using (LifetimeScope.EnqueueParent(_parentScope))
            using (LifetimeScope.Enqueue(
                       builder => builder.RegisterInstance(definition)))
            {
                return UnityEngine.Object.Instantiate(
                    definition.Prefab,
                    position,
                    rotation,
                    _scopeContentRoot.Transform);
            }
        }
    }
}
