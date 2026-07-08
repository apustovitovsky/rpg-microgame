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
        private readonly IPlayerService _player;
        private readonly IWorldLifetimeManager _world;
        private readonly IWorldIdFactory _worldIds;
        private readonly IActorSpawner _actorSpawner;
        private readonly IPickupSpawner _pickupSpawner;
        private readonly ISpawnPointResolver _spawnPoints;

        public GameplayManager(
            ActorSpawnCatalog actors,
            PickupSpawnCatalog pickups,
            IWorldIdFactory worldIds,
            ISpawnPointResolver spawnPoints,
            IWorldLifetimeManager world,
            IActorSpawner actorSpawner,
            IPickupSpawner pickupSpawner,
            IPlayerService player)
        {
            _actors = actors;
            _pickups = pickups;
            _worldIds = worldIds;
            _world = world;
            _actorSpawner = actorSpawner;
            _pickupSpawner = pickupSpawner;
            _player = player;
            _spawnPoints = spawnPoints;
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

            if (!_spawnPoints.TryResolve(
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
                if (!_spawnPoints.TryResolve(
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
                if (!_spawnPoints.TryResolve(
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

        private WorldId SpawnActor(
            ActorSpawnCatalog.ActorEntry entry,
            NavigationNode node,
            bool bindPlayer)
        {
            if (entry == null)
                return default;

            if (entry.Definition == null)
            {
                Debug.LogWarning(
                    "Actor was not spawned: definition is missing.");
                return default;
            }

            if (entry.Definition.Prefab == null)
            {
                Debug.LogWarning(
                    $"Actor '{entry.Definition.name}' was not spawned: prefab is missing.");
                return default;
            }

            var worldId = _worldIds.Create(entry.Definition.DisplayName);

            var request = new ActorSpawnRequest(
                worldId,
                entry.Definition,
                node.Position,
                node.Rotation);

            var actorWorldId = _actorSpawner.Spawn(request);

            if (actorWorldId.IsEmpty)
            {
                Debug.LogWarning(
                    $"Actor '{worldId}' was not spawned.");

                return default;
            }

            if (bindPlayer)
                _player.BindActor(actorWorldId);

            return actorWorldId;
        }

        private WorldId SpawnPickup(
            PickupSpawnCatalog.PickupEntry entry,
            NavigationNode node)
        {
            if (entry == null)
                return default;

            if (entry.Definition == null)
            {
                Debug.LogWarning(
                    "Pickup was not spawned: definition is missing.");
                return default;
            }

            if (entry.Definition.Prefab == null)
            {
                Debug.LogWarning(
                    $"Pickup '{entry.Definition.name}' was not spawned: prefab is missing.");
                return default;
            }

            var worldId = _worldIds.Create(entry.Definition.DisplayName);

            var request = new PickupSpawnRequest(
                worldId,
                entry.Definition,
                node.Position,
                node.Rotation);

            var pickupWorldId = _pickupSpawner.Spawn(request);

            if (pickupWorldId.IsEmpty)
            {
                Debug.LogWarning(
                    $"Pickup '{worldId}' was not spawned.");
            }

            return pickupWorldId;
        }

        public void Dispose()
        {
            _world.DespawnAll();
        }
    }
}