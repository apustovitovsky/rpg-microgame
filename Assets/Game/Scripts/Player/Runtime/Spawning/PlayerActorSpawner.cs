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

        public PlayerActorSpawner(
            IActorSpawner actorSpawner,
            IActorInput input)
        {
            _actorSpawner = actorSpawner;
            _input = input;
        }

        public IActorView Spawn(
            string actorId,
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            var actor = _actorSpawner.Spawn(
                actorId,
                prefab,
                position,
                rotation,
                parent);

            BindInput(actor);

            return actor;
        }

        private void BindInput(IActorView actor)
        {
            if (!actor.TryGet<ActorLookController>(out var look))
                throw MissingCapability<ActorLookController>(actor);

            if (!actor.TryGet<MovementController>(out var movement))
                throw MissingCapability<MovementController>(actor);

            if (!actor.TryGet<TargetingController>(out var targeting))
                throw MissingCapability<TargetingController>(actor);

            look.Bind(_input);
            movement.Bind(_input);
            targeting.Bind(_input);
        }

        private static InvalidOperationException MissingCapability<T>(
            IActorView actor)
        {
            return new InvalidOperationException(
                $"Player actor '{actor.ActorId}' has no required capability '{typeof(T).Name}'.");
        }
    }
}