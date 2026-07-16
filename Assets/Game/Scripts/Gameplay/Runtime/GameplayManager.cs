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
        private readonly IActorSpawner _actorSpawner;
        private readonly IPickupSpawner _pickupSpawner;
        private readonly IPlayerControl _player;
        private readonly ISpawnPointResolver _spawnPoints;

        public GameplayManager(
            ActorSpawnCatalog entitySpawnCatalog,
            PickupSpawnCatalog pickupSpawnCatalog,
            IActorAssetCatalog entityDefinitions,
            IPickupAssetCatalog pickupDefinitions,
            IActorSpawner actorSpawner,
            IPickupSpawner pickupSpawner,
            ISpawnPointResolver spawnPoints,
            IPlayerControl player)
        {
            _entitySpawnCatalog = entitySpawnCatalog;
            _pickupSpawnCatalog = pickupSpawnCatalog;
            _entityDefinitions = entityDefinitions;
            _pickupDefinitions = pickupDefinitions;
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
            var player = _entitySpawnCatalog.Player;

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
            foreach (var actor in _entitySpawnCatalog.Actors)
            {
                if (!_spawnPoints.TryResolve(
                        actor.LocationId,
                        actor.AnchorKey,
                        out var node))
                {
                    Debug.LogWarning(
                        $"Entity '{actor?.DefinitionId}' was not spawned: " +
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

            if (!_entityDefinitions.TryGet(
                    entry.DefinitionId,
                    out var definition))
            {
                Debug.LogWarning(
                    $"Entity definition '{entry.DefinitionId}' was not found.");

                return Guid.Empty;
            }

            var instance = _actorSpawner.Spawn(
                new ActorSpawnRequest(
                    definition,
                    new SpawnPlacement(
                        node.Position,
                        node.Rotation)));

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