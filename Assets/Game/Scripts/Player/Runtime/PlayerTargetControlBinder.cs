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
        private readonly IPlayerInteractionInput _input;
        private readonly IInstanceRegistry<ITargetProvider> _targetProviders;
        private readonly IInstanceRegistry<IPossessable> _possessables;

        public PlayerTargetControlBinder(
            IPlayerInteractionInput input,
            IPlayerService player,
            IInstanceRegistry<ITargetProvider> targetProviders,
            IInstanceRegistry<IPossessable> possessables)
        {
            _input = input;
            _player = player;
            _targetProviders = targetProviders;
            _possessables = possessables;
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

            if (currentActorId == Guid.Empty ||
                !_targetProviders.TryGet(
                    currentActorId,
                    out var targetProvider))
            {
                return;
            }

            var target = targetProvider.CurrentTarget;

            if (target == null ||
                target.InstanceId == Guid.Empty ||
                target.InstanceId == currentActorId)
            {
                return;
            }

            if (!_possessables.Contains(target.InstanceId))
            {
                Debug.LogWarning(
                    $"Target '{target.InstanceId:N}' " +
                    "is not possessable.");

                return;
            }

            _player.BindActor(target.InstanceId);
        }
    }
}