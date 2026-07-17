using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Actor;
using Game.Navigation;
using Game.Pickup;
using Game.Player;
using Game.World;
using UnityEngine;
using VContainer.Unity;

namespace Game.Gameplay
{
    public sealed class GameplayManager :
        IStartable
    {
        private readonly ActorSpawnCatalog _entitySpawnCatalog;
        private readonly PickupSpawnCatalog _pickupSpawnCatalog;
        private readonly IActorAssetCatalog _entityDefinitions;
        private readonly IPickupAssetCatalog _pickupDefinitions;
        private readonly IActorPlacementService _actorPlacements;
        private readonly IActorSpawner _actorSpawner;
        private readonly IPickupSpawner _pickupSpawner;
        private readonly IPlayerControl _player;
        private readonly ISpawnPointResolver _spawnPoints;

        public GameplayManager(
            ActorSpawnCatalog entitySpawnCatalog,
            PickupSpawnCatalog pickupSpawnCatalog,
            IActorAssetCatalog entityDefinitions,
            IPickupAssetCatalog pickupDefinitions,
            IActorPlacementService actorPlacements,
            IActorSpawner actorSpawner,
            IPickupSpawner pickupSpawner,
            ISpawnPointResolver spawnPoints,
            IPlayerControl player)
        {
            _entitySpawnCatalog = entitySpawnCatalog;
            _pickupSpawnCatalog = pickupSpawnCatalog;
            _entityDefinitions = entityDefinitions;
            _pickupDefinitions = pickupDefinitions;
            _actorPlacements = actorPlacements;
            _actorSpawner = actorSpawner;
            _pickupSpawner = pickupSpawner;
            _spawnPoints = spawnPoints;
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
            SpawnActor(
                _entitySpawnCatalog.Player,
                bindPlayer: true);
        }

        private void SpawnActors()
        {
            foreach (var actor in _entitySpawnCatalog.Actors)
            {
                SpawnActor(
                    actor,
                    bindPlayer: false);
            }
        }

        private Guid SpawnActor(
            ActorSpawnCatalog.ActorEntry entry,
            bool bindPlayer)
        {
            if (entry == null)
                return Guid.Empty;

            ActorPlacement placement;

            try
            {
                placement = entry.CreatePlacement();
            }
            catch (InvalidOperationException exception)
            {
                Debug.LogWarning(
                    $"Actor '{entry.DefinitionId}' was not spawned: " +
                    exception.Message);

                return Guid.Empty;
            }

            var spawnLocation = placement.SpawnLocation;

            if (!_spawnPoints.TryResolve(
                    spawnLocation.LocationId,
                    spawnLocation.AnchorKey,
                    out var node))
            {
                Debug.LogWarning(
                    $"Actor '{entry.DefinitionId}' was not spawned: " +
                    "spawn point could not be resolved.");

                return Guid.Empty;
            }

            if (!_entityDefinitions.TryGet(
                    entry.DefinitionId,
                    out var definition))
            {
                Debug.LogWarning(
                    $"Actor definition '{entry.DefinitionId}' was not found.");

                return Guid.Empty;
            }

            var instanceId = Guid.NewGuid();

            _actorPlacements.Register(
                instanceId,
                placement);

            ActorInstance instance;

            try
            {
                instance = _actorSpawner.Spawn(
                    new ActorSpawnRequest(
                        definition,
                        new SpawnPlacement(
                            node.Position,
                            node.Rotation),
                        instanceId));
            }
            catch
            {
                _actorPlacements.Unregister(instanceId);
                throw;
            }

            if (bindPlayer)
            {
                _player.PossessAsync(
                        instance.InstanceId,
                        CancellationToken.None)
                    .Forget();
            }

            return instance.InstanceId;
        }

        private void SpawnPickups()
        {
            foreach (var pickup in _pickupSpawnCatalog.Pickups)
                SpawnPickup(pickup);
        }

        private Guid SpawnPickup(
            PickupSpawnCatalog.PickupEntry entry)
        {
            if (entry == null)
                return Guid.Empty;

            if (!_pickupDefinitions.TryGet(
                    entry.DefinitionId,
                    out var definition))
            {
                Debug.LogWarning(
                    $"Pickup definition '{entry.DefinitionId}' was not found.");

                return Guid.Empty;
            }

            if (!_spawnPoints.TryResolve(
                    entry.LocationId,
                    entry.AnchorKey,
                    out var node))
            {
                Debug.LogWarning(
                    $"Pickup '{definition.Id}' was not spawned: " +
                    "spawn point could not be resolved.");

                return Guid.Empty;
            }

            var instance = _pickupSpawner.Spawn(
                new PickupSpawnRequest(
                    definition,
                    new SpawnPlacement(
                        node.Position,
                        node.Rotation)));

            return instance.InstanceId;
        }
    }
}