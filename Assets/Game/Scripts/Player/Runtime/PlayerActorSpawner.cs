using Game.Actor;
using Game.World;
using UnityEngine;

namespace Game.Player
{
    public sealed class PlayerActorSpawner : IPlayerActorSpawner
    {
        private readonly IActorSpawner _actorSpawner;
        private readonly IPlayerService _player;

        public PlayerActorSpawner(
            IActorSpawner actorSpawner,
            IPlayerService player)
        {
            _actorSpawner = actorSpawner;
            _player = player;
        }

        public IWorldObject Spawn(
            WorldId worldId,
            string displayName,
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            var actor = _actorSpawner.Spawn(
                worldId,
                displayName,
                prefab,
                position,
                rotation,
                parent);

            _player.BindActor(actor);

            return actor;
        }
    }
}