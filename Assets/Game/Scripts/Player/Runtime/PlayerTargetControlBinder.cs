using System;
using Game.Actor;
using Game.Targeting;
using Game.World;
using UnityEngine;
using VContainer.Unity;

namespace Game.Player
{
    public sealed class PlayerTargetControlBinder :
        IStartable,
        IDisposable
    {
        private readonly IPlayerService _player;
        private readonly IWorldObjectRegistry _worldObjects;
        private readonly IPlayerInteractionInput _input;

        public PlayerTargetControlBinder(
            IPlayerInteractionInput input,
            IPlayerService player,
            IWorldObjectRegistry worldObjects)
        {
            _input = input;
            _player = player;
            _worldObjects = worldObjects;
        }

        public void Start()
        {
            _input.PossessPerformed += BindCurrentTarget;
        }

        public void Dispose()
        {
            _input.PossessPerformed -= BindCurrentTarget;
        }

        private void BindCurrentTarget()
        {
            var currentActor = _player.CurrentActor;

            if (currentActor == null ||
                !currentActor.TryGet<ITargetProvider>(out var targetProvider))
            {
                return;
            }

            var target = targetProvider.CurrentTarget;

            if (target == null ||
                target.WorldId.IsEmpty)
            {
                return;
            }

            if (target.WorldId == currentActor.WorldId)
                return;

            if (!_worldObjects.TryGet(
                    target.WorldId,
                    out var targetObject))
            {
                Debug.LogWarning(
                    $"Target '{target.WorldId}' is not registered.");

                return;
            }

            if (!targetObject.TryGet<IActorInputBinder>(out _))
            {
                Debug.LogWarning(
                    $"Target '{target.WorldId}' is not a controllable actor.");

                return;
            }

            _player.BindActor(targetObject);
        }
    }
}