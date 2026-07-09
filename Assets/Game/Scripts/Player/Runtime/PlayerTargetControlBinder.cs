using System;
using Game.Actor;
using UnityEngine;
using VContainer.Unity;

namespace Game.Player
{
    public sealed class PlayerTargetControlBinder :
        IStartable,
        IDisposable
    {
        private readonly IPlayerService _player;
        private readonly IActorService _actors;
        private readonly IPlayerInteractionInput _input;

        public PlayerTargetControlBinder(
            IPlayerInteractionInput input,
            IPlayerService player,
            IActorService actors)
        {
            _input = input;
            _player = player;
            _actors = actors;
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
            var currentActorId = _player.CurrentActor;

            if (currentActorId.IsEmpty ||
                !_actors.TryGet(currentActorId, out var currentActor) ||
                currentActor.Targeting == null)
            {
                return;
            }

            var target = currentActor.Targeting.CurrentTarget;

            if (target == null ||
                target.WorldId.IsEmpty)
            {
                return;
            }

            if (target.WorldId == currentActorId)
                return;

            if (!_actors.TryGet(target.WorldId, out var actor) ||
                actor.InputBinder == null)
            {
                Debug.LogWarning(
                    $"Target '{target.WorldId}' is not a controllable actor.");

                return;
            }

            _player.BindActor(target.WorldId);
        }
    }
}