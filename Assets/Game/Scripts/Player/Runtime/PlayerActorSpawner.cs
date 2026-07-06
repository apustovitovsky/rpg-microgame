using System;
using Game.Actor;
using Game.Input;
using UnityEngine;

namespace Game.Player
{
    public sealed class PlayerActorSpawner : IPlayerActorSpawner
    {
        private readonly IActorSpawner _actorSpawner;
        private readonly IActorInput _input;
        private readonly IPlayerService _player;

        public PlayerActorSpawner(
            IActorSpawner actorSpawner,
            IActorInput input,
            IPlayerService player)
        {
            _actorSpawner = actorSpawner;
            _input = input;
            _player = player;
        }

        public ActorInstance Spawn(
            string instanceId,
            string definitionId,
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            var actor = _actorSpawner.Spawn(
                instanceId,
                definitionId,
                prefab,
                position,
                rotation,
                parent);

            BindInput(actor);

            return actor;
        }

        private void BindInput(ActorInstance actor)
        {
            if (actor.InputBinder == null)
                throw MissingCapability<IActorInputBinder>(actor);

            actor.InputBinder.Bind(_input);
            _player.BindActor(actor);
        }

        private static InvalidOperationException MissingCapability<T>(
            ActorInstance actor)
        {
            return new InvalidOperationException(
                $"Player actor '{actor.InstanceId}' has no required capability '{typeof(T).Name}'.");
        }
    }
}