using System;
using System.Collections.Generic;
using Etheria.Game.World;
using Game.Actor;
using Game.Player;
using UnityEngine;
using VContainer.Unity;

namespace Game.World
{
    public sealed class WorldActorLifecycleManager :
        IStartable,
        IDisposable
    {
        private readonly WorldActorConfigSO _manifest;
        private readonly INavigationLocationResolver _locations;
        private readonly INavigationGraphProvider _graphProvider;
        private readonly IActorSpawner _actorSpawner;
        private readonly IPlayerActorSpawner _playerSpawner;
        private readonly IActorRegistryWriter _actorRegistry;

        private readonly Dictionary<string, ActorInstance> _spawned =
            new(StringComparer.Ordinal);

        private int _nextActorInstanceIndex;

        public WorldActorLifecycleManager(
            WorldActorConfigSO manifest,
            INavigationLocationResolver locations,
            INavigationGraphProvider graphProvider,
            IActorSpawner actorSpawner,
            IPlayerActorSpawner playerSpawner,
            IActorRegistryWriter actorRegistry)
        {
            _manifest = manifest;
            _locations = locations;
            _graphProvider = graphProvider;
            _actorSpawner = actorSpawner;
            _playerSpawner = playerSpawner;
            _actorRegistry = actorRegistry;
        }

        public void Start()
        {
            SpawnPlayer();
            SpawnActors();
        }

        private void SpawnPlayer()
        {
            var player = _manifest.Player;

            if (!TryResolveSpawnPoint(
                    player,
                    out var node))
            {
                Debug.LogWarning(
                    "Player was not spawned: spawn point could not be resolved.");
                return;
            }

            Spawn(
                player,
                node,
                usePlayerSpawner: true);
        }

        private void SpawnActors()
        {
            foreach (var actor in _manifest.Actors)
            {
                if (!TryResolveSpawnPoint(
                        actor,
                        out var node))
                {
                    Debug.LogWarning(
                        $"Actor '{actor?.DefinitionId}' was not spawned: spawn point could not be resolved.");
                    continue;
                }

                Spawn(
                    actor,
                    node,
                    usePlayerSpawner: false);
            }
        }

        private ActorInstance Spawn(
            WorldActorConfigSO.ActorEntry entry,
            NavigationNode node,
            bool usePlayerSpawner)
        {
            if (entry == null)
                return null;

            var definitionId = entry.DefinitionId?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(definitionId))
            {
                Debug.LogWarning("Actor was not spawned: actor definition id is empty.");
                return null;
            }

            if (entry.Prefab == null)
            {
                Debug.LogWarning(
                    $"Actor '{definitionId}' was not spawned: prefab is missing.");
                return null;
            }

            var instanceId = CreateInstanceId(definitionId);

            if (_spawned.ContainsKey(instanceId))
            {
                Debug.LogWarning(
                    $"Actor '{instanceId}' was not spawned: actor instance is already spawned.");
                return null;
            }

            var actor = usePlayerSpawner
                ? _playerSpawner.Spawn(
                    instanceId,
                    definitionId,
                    entry.Prefab,
                    node.Position,
                    node.Rotation)
                : _actorSpawner.Spawn(
                    instanceId,
                    definitionId,
                    entry.Prefab,
                    node.Position,
                    node.Rotation);

            _actorRegistry.Register(actor);

            _spawned.Add(
                instanceId,
                actor);

            return actor;
        }

        private string CreateInstanceId(string definitionId)
        {
            _nextActorInstanceIndex++;
            return $"{definitionId}_{_nextActorInstanceIndex:0000}";
        }

        private bool TryResolveSpawnPoint(
            WorldActorConfigSO.ActorEntry entry,
            out NavigationNode node)
        {
            node = null;

            if (entry == null ||
                string.IsNullOrWhiteSpace(entry.LocationId) ||
                string.IsNullOrWhiteSpace(entry.AnchorKey))
            {
                return false;
            }

            if (_graphProvider.Graph == null)
            {
                return false;
            }

            if (!_locations.TryResolveAnchorNodeId(
                    entry.LocationId,
                    entry.AnchorKey,
                    out var nodeId))
            {
                return false;
            }

            return _graphProvider.Graph.TryGetNode(
                nodeId,
                out node);
        }

        public void Dispose()
        {
            DespawnAll();
        }

        public bool Despawn(string instanceId)
        {
            instanceId = instanceId?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(instanceId))
                return false;

            if (!_spawned.TryGetValue(instanceId, out var actor))
                return false;

            _spawned.Remove(instanceId);

            if (actor == null)
                return false;

            _actorRegistry.Unregister(actor);

            if (actor.View is Component component && component != null)
            {
                UnityEngine.Object.Destroy(component.gameObject);
            }

            return true;
        }

        private void DespawnAll()
        {
            var instanceIds = new List<string>(_spawned.Keys);

            foreach (var instanceId in instanceIds)
                Despawn(instanceId);

            _spawned.Clear();
        }
    }
}