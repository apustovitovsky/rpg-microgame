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
        private readonly ActorSpawnCatalog _actors;
        private readonly PickupSpawnCatalog _pickups;
        private readonly LootContainerSpawnCatalog _lootContainers;
        private readonly IActorDefinitionCatalog _actorDefinitions;
        private readonly IPlayerControl _player;
        private readonly ISpawnedObjectRegistry _spawnedObjects;
        private readonly IActorSpawner _actorSpawner;
        private readonly IPickupSpawner _pickupSpawner;
        private readonly ILootContainerSpawner _lootContainerSpawner;
        private readonly ISpawnPointResolver _spawnPoints;

        public GameplayManager(
            ActorSpawnCatalog actors,
            PickupSpawnCatalog pickups,
            LootContainerSpawnCatalog lootContainers,
            IActorDefinitionCatalog actorDefinitions,
            ISpawnPointResolver spawnPoints,
            ISpawnedObjectRegistry spawnedObjects,
            IActorSpawner actorSpawner,
            IPickupSpawner pickupSpawner,
            ILootContainerSpawner lootContainerSpawner,
            IPlayerControl player)
        {
            _actors = actors;
            _pickups = pickups;
            _lootContainers = lootContainers;
            _actorDefinitions = actorDefinitions;
            _spawnedObjects = spawnedObjects;
            _actorSpawner = actorSpawner;
            _pickupSpawner = pickupSpawner;
            _lootContainerSpawner = lootContainerSpawner;
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

            if (!_actorDefinitions.TryGet(
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
                    $"Actor '{definition.DefinitionId}' was not spawned: " +
                    "prefab is missing.");

                return Guid.Empty;
            }

            var actorInstance = new ActorInstance(definition);

            var request = new ActorSpawnRequest(
                actorInstance,
                node.Position,
                node.Rotation);

            var actorInstanceId =
                _actorSpawner.Spawn(request);

            if (actorInstanceId == Guid.Empty)
            {
                Debug.LogWarning(
                    $"Actor '{actorInstance.InstanceId:N}' was not spawned.");

                return Guid.Empty;
            }

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

            if (entry.Definition == null)
            {
                Debug.LogWarning(
                    "Pickup was not spawned: definition is missing.");

                return Guid.Empty;
            }

            if (entry.Definition.Prefab == null)
            {
                Debug.LogWarning(
                    $"Pickup '{entry.Definition.name}' was not spawned: " +
                    "prefab is missing.");

                return Guid.Empty;
            }

            if (!_spawnPoints.TryResolve(
                    entry.LocationId,
                    entry.AnchorKey,
                    out var node))
            {
                Debug.LogWarning(
                    $"Pickup '{entry.Definition.name}' was not spawned: " +
                    "spawn point could not be resolved.");

                return Guid.Empty;
            }

            var pickupInstance = new PickupInstance(
                entry.Definition);

            var request = new PickupSpawnRequest(
                pickupInstance,
                node.Position,
                node.Rotation);

            var spawnedPickupId =
                _pickupSpawner.Spawn(request);

            if (spawnedPickupId == Guid.Empty)
            {
                Debug.LogWarning(
                    $"Pickup '{pickupInstance.InstanceId:N}' was not spawned.");
            }

            return spawnedPickupId;
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

            if (entry.Definition == null)
            {
                Debug.LogWarning(
                    "Loot container was not spawned: definition is missing.");

                return Guid.Empty;
            }

            if (entry.Definition.Prefab == null)
            {
                Debug.LogWarning(
                    $"Loot container '{entry.Definition.name}' " +
                    "was not spawned: prefab is missing.");

                return Guid.Empty;
            }

            if (!_spawnPoints.TryResolve(
                    entry.LocationId,
                    entry.AnchorKey,
                    out var node))
            {
                Debug.LogWarning(
                    $"Loot container '{entry.Definition.name}' " +
                    "was not spawned: spawn point could not be resolved.");

                return Guid.Empty;
            }

            var containerInstance = new LootContainerInstance(
                entry.Definition);

            var request = new LootContainerSpawnRequest(
                containerInstance,
                node.Position,
                node.Rotation);

            var spawnedContainerId =
                _lootContainerSpawner.Spawn(request);

            if (spawnedContainerId == Guid.Empty)
            {
                Debug.LogWarning(
                    $"Loot container '{containerInstance.InstanceId:N}' " +
                    "was not spawned.");
            }

            return spawnedContainerId;
        }

        public void Dispose()
        {
            _spawnedObjects.DespawnAll();
        }
    }
}