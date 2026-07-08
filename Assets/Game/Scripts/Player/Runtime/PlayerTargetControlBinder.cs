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
        private readonly IWorldManager _world;
        private readonly IWorldRegistry<IActorInputBinder> _inputBinders;
        private readonly IWorldRegistry<ITargetProvider> _targetProviders;
        private readonly IPlayerInteractionInput _input;

        public PlayerTargetControlBinder(
            IPlayerInteractionInput input,
            IPlayerService player,
            IWorldManager world,
            IWorldRegistry<IActorInputBinder> inputBinders,
            IWorldRegistry<ITargetProvider> targetProviders)
        {
            _input = input;
            _player = player;
            _world = world;
            _inputBinders = inputBinders;
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

            if (currentActor == null ||
                !_targetProviders.TryGet(currentActor.WorldId, out var targetProvider))
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

            if (!_world.TryGetHandle(target.WorldId, out var targetHandle))
            {
                Debug.LogWarning(
                    $"Target '{target.WorldId}' is not tracked.");

                return;
            }

            if (!_inputBinders.Contains(target.WorldId))
            {
                Debug.LogWarning(
                    $"Target '{target.WorldId}' is not a controllable actor.");

                return;
            }

            _player.BindActor(targetHandle);
        }
    }
}