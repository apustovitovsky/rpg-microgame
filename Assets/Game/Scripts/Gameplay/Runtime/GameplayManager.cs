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
        private readonly IActorDefinitionCatalog _actorDefinitions;
        private readonly IPlayerService _player;
        private readonly IWorldObjectRegistry _world;
        private readonly IWorldIdFactory _worldIds;
        private readonly IActorSpawner _actorSpawner;
        private readonly IPickupSpawner _pickupSpawner;
        private readonly ISpawnPointResolver _spawnPoints;

        public GameplayManager(
            ActorSpawnCatalog actors,
            PickupSpawnCatalog pickups,
            IActorDefinitionCatalog actorDefinitions,
            IWorldIdFactory worldIds,
            ISpawnPointResolver spawnPoints,
            IWorldObjectRegistry world,
            IActorSpawner actorSpawner,
            IPickupSpawner pickupSpawner,
            IPlayerService player)
        {
            _actors = actors;
            _pickups = pickups;
            _actorDefinitions = actorDefinitions;
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

        private WorldId SpawnActor(
            ActorSpawnCatalog.ActorEntry entry,
            NavigationNode node,
            bool bindPlayer)
        {
            if (entry == null)
                return default;

            if (!_actorDefinitions.TryGet(
                    entry.DefinitionId,
                    out var definition))
            {
                Debug.LogWarning(
                    $"Actor definition '{entry.DefinitionId}' was not found.");

                return default;
            }

            if (definition.Prefab == null)
            {
                Debug.LogWarning(
                    $"Actor '{definition.DefinitionId}' was not spawned: " +
                    "prefab is missing.");

                return default;
            }

            var worldId =
                _worldIds.Create(definition.DefinitionId);

            var request = new ActorSpawnRequest(
                worldId,
                definition,
                node.Position,
                node.Rotation);

            var actorWorldId =
                _actorSpawner.Spawn(request);

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

        private void SpawnPickups()
        {
            foreach (var pickup in _pickups.Pickups)
                SpawnPickup(pickup);
        }

        private WorldId SpawnPickup(
            PickupSpawnCatalog.PickupEntry entry)
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
                    $"Pickup '{entry.Definition.name}' was not spawned: " +
                    "prefab is missing.");

                return default;
            }

            if (!_spawnPoints.TryResolve(
                    entry.LocationId,
                    entry.AnchorKey,
                    out var node))
            {
                Debug.LogWarning(
                    $"Pickup '{entry.Definition.name}' was not spawned: " +
                    "spawn point could not be resolved.");

                return default;
            }

            var worldId =
                _worldIds.Create(entry.Definition.DisplayName);

            var request = new PickupSpawnRequest(
                worldId,
                entry.Definition,
                node.Position,
                node.Rotation);

            var pickupWorldId =
                _pickupSpawner.Spawn(request);

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