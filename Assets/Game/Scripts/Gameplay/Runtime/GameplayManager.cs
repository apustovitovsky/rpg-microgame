using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Game.World;
using Game.Actor;
using Game.Loot;
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
        private readonly ActorSpawnCatalog _spawnCatalog;
        private readonly PickupSpawnCatalog _pickups;
        private readonly LootContainerSpawnCatalog _lootContainers;
        private readonly IActorAssetCatalog _actorCatalog;
        private readonly IPickupAssetCatalog _pickupCatalog;
        private readonly ILootContainerAssetCatalog _lootContainerCatalog;
        private readonly IPlayerControl _player;
        private readonly ISpawnedObjectRegistry _spawnedObjects;
        private readonly IWorldSpawner _worldSpawner;
        private readonly ISpawnPointResolver _spawnPoints;

        public GameplayManager(
            ActorSpawnCatalog spawnCatalog,
            PickupSpawnCatalog pickups,
            LootContainerSpawnCatalog lootContainers,
            IActorAssetCatalog actorCatalog,
            IPickupAssetCatalog pickupCatalog,
            ILootContainerAssetCatalog lootContainerCatalog,
            ISpawnPointResolver spawnPoints,
            ISpawnedObjectRegistry spawnedObjects,
            IWorldSpawner worldSpawner,
            IPlayerControl player)
        {
            _spawnCatalog = spawnCatalog;
            _pickups = pickups;
            _lootContainers = lootContainers;
            _actorCatalog = actorCatalog;
            _pickupCatalog = pickupCatalog;
            _lootContainerCatalog = lootContainerCatalog;
            _spawnedObjects = spawnedObjects;
            _worldSpawner = worldSpawner;
            _player = player;
            _spawnPoints = spawnPoints;
        }

        public void Start()
        {
            SpawnPlayer();
            SpawnActors();
            SpawnPickups();
            SpawnLootContainers();
        }

        private void SpawnPlayer()
        {
            var player = _spawnCatalog.Player;

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
            foreach (var actor in _spawnCatalog.Actors)
            {
                if (!_spawnPoints.TryResolve(
                        actor.LocationId,
                        actor.AnchorKey,
                        out var node))
                {
                    Debug.LogWarning(
                        $"Actor '{actor?.DefinitionId}' was not spawned: " +
                        "spawn point could not be resolved.");

                    continue;
                }

                SpawnActor(
                    actor,
                    node,
                    bindPlayer: false);
            }
        }

        private Guid SpawnActor(
            ActorSpawnCatalog.ActorEntry entry,
            NavigationNode node,
            bool bindPlayer)
        {
            if (entry == null)
                return Guid.Empty;

            if (!_actorCatalog.TryGet(
                    entry.DefinitionId,
                    out var definition))
            {
                Debug.LogWarning(
                    $"Actor definition '{entry.DefinitionId}' was not found.");

                return Guid.Empty;
            }

            if (definition.Prefab == null)
            {
                Debug.LogWarning(
                    $"Actor '{definition.Id}' was not spawned: " +
                    "prefab is missing.");

                return Guid.Empty;
            }

            var spawnedObject = _worldSpawner.Spawn(
                new SpawnRequest<ActorInstance>(
                    definition,
                    new SpawnPlacement(
                        node.Position,
                        node.Rotation)));

            var actorInstanceId = spawnedObject.InstanceId;

            if (bindPlayer)
            {
                _player.PossessAsync(
                        actorInstanceId,
                        CancellationToken.None)
                    .Forget();
            }

            return actorInstanceId;
        }

        private void SpawnPickups()
        {
            foreach (var pickup in _pickups.Pickups)
                SpawnPickup(pickup);
        }

        private Guid SpawnPickup(
            PickupSpawnCatalog.PickupEntry entry)
        {
            if (entry == null)
                return Guid.Empty;

            if (!_pickupCatalog.TryGet(
                    entry.DefinitionId,
                    out var definition))
            {
                Debug.LogWarning(
                    $"Pickup definition '{entry.DefinitionId}' was not found.");

                return Guid.Empty;
            }

            if (definition.Prefab == null)
            {
                Debug.LogWarning(
                    $"Pickup '{definition.Id}' was not spawned: " +
                    "prefab is missing.");

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

            var spawnedObject = _worldSpawner.Spawn(
                new SpawnRequest<PickupInstance>(
                    definition,
                    new SpawnPlacement(
                        node.Position,
                        node.Rotation)));

            return spawnedObject.InstanceId;
        }

        private void SpawnLootContainers()
        {
            foreach (var container in _lootContainers.Containers)
                SpawnLootContainer(container);
        }

        private Guid SpawnLootContainer(
            LootContainerSpawnCatalog.LootContainerEntry entry)
        {
            if (entry == null)
                return Guid.Empty;

            if (!_lootContainerCatalog.TryGet(
                    entry.DefinitionId,
                    out var definition))
            {
                Debug.LogWarning(
                    $"Loot container definition '{entry.DefinitionId}' " +
                    "was not found.");

                return Guid.Empty;
            }

            if (definition.Prefab == null)
            {
                Debug.LogWarning(
                    $"Loot container '{definition.Id}' " +
                    "was not spawned: prefab is missing.");

                return Guid.Empty;
            }

            if (!_spawnPoints.TryResolve(
                    entry.LocationId,
                    entry.AnchorKey,
                    out var node))
            {
                Debug.LogWarning(
                    $"Loot container '{definition.Id}' " +
                    "was not spawned: spawn point could not be resolved.");

                return Guid.Empty;
            }

            var spawnedObject = _worldSpawner.Spawn(
                new SpawnRequest<LootContainerInstance>(
                    definition,
                    new SpawnPlacement(
                        node.Position,
                        node.Rotation)));

            return spawnedObject.InstanceId;
        }

        public void Dispose()
        {
            _spawnedObjects.DespawnAll();
        }
    }
}