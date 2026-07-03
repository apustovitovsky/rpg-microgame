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


        private readonly Dictionary<string, IActorView> _spawned =
            new(StringComparer.Ordinal);

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
                        $"Actor '{actor?.ActorId}' was not spawned: spawn point could not be resolved.");
                    continue;
                }

                Spawn(
                    actor,
                    node,
                    usePlayerSpawner: false);
            }
        }

        private IActorView Spawn(
            WorldActorConfigSO.ActorEntry entry,
            NavigationNode node,
            bool usePlayerSpawner)
        {
            if (entry == null)
                return null;

            if (string.IsNullOrWhiteSpace(entry.ActorId))
            {
                Debug.LogWarning("Actor was not spawned: actor id is empty.");
                return null;
            }

            if (_spawned.ContainsKey(entry.ActorId))
            {
                Debug.LogWarning(
                    $"Actor '{entry.ActorId}' was not spawned: actor is already spawned.");
                return null;
            }

            if (entry.Prefab == null)
            {
                Debug.LogWarning(
                    $"Actor '{entry.ActorId}' was not spawned: prefab is missing.");
                return null;
            }

            var view = usePlayerSpawner
                ? _playerSpawner.Spawn(
                    entry.ActorId,
                    entry.Prefab,
                    node.Position,
                    node.Rotation)
                : _actorSpawner.Spawn(
                    entry.ActorId,
                    entry.Prefab,
                    node.Position,
                    node.Rotation);

            _actorRegistry.Register(view);

            _spawned.Add(
                entry.ActorId,
                view);

            return view;
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

        public bool Despawn(string actorId)
        {
            actorId = actorId?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(actorId))
                return false;

            if (!_spawned.TryGetValue(actorId, out var actor))
                return false;

            _spawned.Remove(actorId);

            if (actor == null)
                return false;

            _actorRegistry.Unregister(actor);

            if (actor is Component component && component != null)
            {
                UnityEngine.Object.Destroy(component.gameObject);
            }

            return true;
        }

        private void DespawnAll()
        {
            var actorIds = new List<string>(_spawned.Keys);

            foreach (var actorId in actorIds)
                Despawn(actorId);

            _spawned.Clear();
        }
    }
}