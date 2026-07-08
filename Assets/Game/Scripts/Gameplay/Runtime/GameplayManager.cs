using System;
using Etheria.Game.World;
using Game.Actor;
using Game.Pickup;
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
        private readonly ActorSpawnCatalog _actors;
        private readonly PickupSpawnCatalog _pickups;
        private readonly INavigationLocationResolver _locations;
        private readonly INavigationGraphProvider _graphProvider;
        private readonly IWorldSpawner _worldSpawner;
        private readonly IWorldObjectFactory<ActorSpawnRequest> _actorFactory;
        private readonly IWorldObjectFactory<PickupSpawnRequest> _pickupFactory;
        private readonly IPlayerService _player;
        private readonly IWorldManager _world;

        private int _nextWorldIdIndex;

        public GameplayManager(
            ActorSpawnCatalog actors,
            PickupSpawnCatalog pickups,
            INavigationLocationResolver locations,
            INavigationGraphProvider graphProvider,
            IWorldSpawner worldSpawner,
            IWorldManager world,
            IWorldObjectFactory<ActorSpawnRequest> actorFactory,
            IWorldObjectFactory<PickupSpawnRequest> pickupFactory,
            IPlayerService player)
        {
            _actors = actors;
            _pickups = pickups;
            _locations = locations;
            _graphProvider = graphProvider;
            _worldSpawner = worldSpawner;
            _world = world;
            _actorFactory = actorFactory;
            _pickupFactory = pickupFactory;
            _player = player;
        }

        public void Start()
        {
            SpawnPlayer();
            SpawnActors();
            SpawnPickups();
        }

        private void SpawnPlayer()
        {
            var player = _actors.Player;

            if (!TryResolveSpawnPoint(
                    player.LocationId,
                    player.AnchorKey,
                    out var node))
            {
                Debug.LogWarning(
                    "Player was not spawned: spawn point could not be resolved.");
                return;
            }

            SpawnActor(
                player,
                node,
                bindPlayer: true);
        }

        private void SpawnActors()
        {
            foreach (var actor in _actors.Actors)
            {
                if (!TryResolveSpawnPoint(
                        actor.LocationId,
                        actor.AnchorKey,
                        out var node))
                {
                    Debug.LogWarning(
                        $"Actor '{actor?.Definition?.name}' was not spawned: spawn point could not be resolved.");
                    continue;
                }

                SpawnActor(
                    actor,
                    node,
                    bindPlayer: false);
            }
        }

        private void SpawnPickups()
        {
            foreach (var pickup in _pickups.Pickups)
            {
                if (!TryResolveSpawnPoint(
                        pickup.LocationId,
                        pickup.AnchorKey,
                        out var node))
                {
                    Debug.LogWarning(
                        $"Pickup '{pickup?.Definition?.name}' was not spawned: spawn point could not be resolved.");
                    continue;
                }

                SpawnPickup(
                    pickup,
                    node);
            }
        }

        private IWorldObject SpawnActor(
            ActorSpawnCatalog.ActorEntry entry,
            NavigationNode node,
            bool bindPlayer)
        {
            if (entry == null)
                return null;

            if (entry.Definition == null)
            {
                Debug.LogWarning(
                    "Actor was not spawned: definition is missing.");
                return null;
            }

            if (entry.Definition.Prefab == null)
            {
                Debug.LogWarning(
                    $"Actor '{entry.Definition.name}' was not spawned: prefab is missing.");
                return null;
            }

            var worldId = CreateWorldId(entry.Definition.DisplayName);

            var request = new ActorSpawnRequest(
                worldId,
                entry.Definition,
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

        private IWorldObject SpawnPickup(
            PickupSpawnCatalog.PickupEntry entry,
            NavigationNode node)
        {
            if (entry == null)
                return null;

            if (entry.Definition == null)
            {
                Debug.LogWarning(
                    "Pickup was not spawned: definition is missing.");
                return null;
            }

            if (entry.Definition.Prefab == null)
            {
                Debug.LogWarning(
                    $"Pickup '{entry.Definition.name}' was not spawned: prefab is missing.");
                return null;
            }

            var worldId = CreateWorldId(entry.Definition.DisplayName);

            var request = new PickupSpawnRequest(
                worldId,
                entry.Definition,
                node.Position,
                node.Rotation);

            var pickup = _worldSpawner.Spawn(
                request,
                _pickupFactory);

            if (pickup == null)
            {
                Debug.LogWarning(
                    $"Pickup '{worldId}' was not spawned.");
            }

            return pickup;
        }

        private WorldId CreateWorldId(string prefix)
        {
            _nextWorldIdIndex++;

            return new WorldId(
                $"{NormalizeWorldIdPrefix(prefix)}_{_nextWorldIdIndex:000}");
        }

        private static string NormalizeWorldIdPrefix(string value)
        {
            value = value?.Trim().ToLowerInvariant() ?? "world_object";

            if (string.IsNullOrWhiteSpace(value))
                return "world_object";

            var chars = value.ToCharArray();

            for (var i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]))
                    chars[i] = '_';
            }

            return new string(chars);
        }

        private bool TryResolveSpawnPoint(
            string locationId,
            string anchorKey,
            out NavigationNode node)
        {
            node = null;

            if (string.IsNullOrWhiteSpace(locationId) ||
                string.IsNullOrWhiteSpace(anchorKey))
            {
                return false;
            }

            if (_graphProvider.Graph == null)
                return false;

            if (!_locations.TryResolveAnchorNodeId(
                    locationId,
                    anchorKey,
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