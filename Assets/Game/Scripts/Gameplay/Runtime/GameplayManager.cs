using System;
using Etheria.Game.World;
using Game.Actor;
using Game.Player;
using Game.World;
using UnityEngine;
using VContainer.Unity;

namespace Game.Gameplay
{
    public sealed class GameplayManager :
        IStartable,
        IDisposable
    {
        private readonly GameplayActorConfigSO _manifest;
        private readonly INavigationLocationResolver _locations;
        private readonly INavigationGraphProvider _graphProvider;
        private readonly IWorldSpawner _worldSpawner;
        private readonly IWorldObjectFactory<ActorSpawnRequest> _actorFactory;
        private readonly IPlayerService _player;
        private readonly IWorldManager _world;

        private int _nextWorldIdIndex;

        public GameplayManager(
            GameplayActorConfigSO manifest,
            INavigationLocationResolver locations,
            INavigationGraphProvider graphProvider,
            IWorldSpawner worldSpawner,
            IWorldManager world,
            IWorldObjectFactory<ActorSpawnRequest> actorFactory,
            IPlayerService player)
        {
            _manifest = manifest;
            _locations = locations;
            _graphProvider = graphProvider;
            _worldSpawner = worldSpawner;
            _world = world;
            _actorFactory = actorFactory;
            _player = player;
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
                bindPlayer: true);
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
                        $"Actor '{actor?.DisplayName}' was not spawned: spawn point could not be resolved.");
                    continue;
                }

                Spawn(
                    actor,
                    node,
                    bindPlayer: false);
            }
        }

        private IWorldObject Spawn(
            GameplayActorConfigSO.ActorEntry entry,
            NavigationNode node,
            bool bindPlayer)
        {
            if (entry == null)
                return null;

            if (entry.Prefab == null)
            {
                Debug.LogWarning(
                    $"Actor '{entry.DisplayName}' was not spawned: prefab is missing.");
                return null;
            }

            var worldId = CreateWorldId(entry);

            var request = new ActorSpawnRequest(
                worldId,
                entry.DisplayName,
                entry.Prefab,
                node.Position,
                node.Rotation);

            var actor = _worldSpawner.Spawn(
                request,
                _actorFactory);

            if (actor == null)
            {
                Debug.LogWarning(
                    $"Actor '{worldId}' was not spawned.");

                return null;
            }

            if (bindPlayer)
                _player.BindActor(actor);

            return actor;
        }

        private WorldId CreateWorldId(GameplayActorConfigSO.ActorEntry entry)
        {
            _nextWorldIdIndex++;

            var prefix = !string.IsNullOrWhiteSpace(entry.DisplayName)
                ? entry.DisplayName
                : entry.Prefab.name;

            return new WorldId(
                $"{NormalizeWorldIdPrefix(prefix)}_{_nextWorldIdIndex:0000}");
        }

        private static string NormalizeWorldIdPrefix(string value)
        {
            value = value?.Trim().ToLowerInvariant() ?? "actor";

            if (string.IsNullOrWhiteSpace(value))
                return "actor";

            var chars = value.ToCharArray();

            for (var i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]))
                    chars[i] = '_';
            }

            return new string(chars);
        }

        private bool TryResolveSpawnPoint(
            GameplayActorConfigSO.ActorEntry entry,
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
                return false;

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
            _world.DespawnAll();
        }
    }
}