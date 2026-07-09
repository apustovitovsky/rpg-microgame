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
        private readonly IWorldRegistry<IWorldActor> _actors;
        private readonly IWorldRegistry<ITargetProvider> _targetProviders;
        private readonly IPlayerInteractionInput _input;

        public PlayerTargetControlBinder(
            IPlayerInteractionInput input,
            IPlayerService player,
            IWorldRegistry<IWorldActor> actors,
            IWorldRegistry<ITargetProvider> targetProviders)
        {
            _input = input;
            _player = player;
            _actors = actors;
            _targetProviders = targetProviders;
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

            if (currentActor.IsEmpty ||
                !_targetProviders.TryGet(currentActor, out var targetProvider))
            {
                return;
            }

            var target = targetProvider.CurrentTarget;

            if (target == null ||
                target.WorldId.IsEmpty)
            {
                return;
            }

            if (target.WorldId == currentActor)
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