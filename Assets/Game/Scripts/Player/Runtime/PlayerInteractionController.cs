using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.CommandSystem;
using Game.Core;
using Game.Interaction;
using Game.Targeting;
using Game.World;
using VContainer.Unity;

namespace Game.Player
{
    public sealed class PlayerInteractionController :
        IStartable,
        IDisposable
    {
        private readonly IPlayerInteractionInput _input;
        private readonly IPlayerService _player;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IInstanceRegistry<ITargetProvider> _targetProviders;
        private readonly ISpawnedObjectRegistry _spawnedObjects;

        private CancellationTokenSource _interactionCts;

        public PlayerInteractionController(
            IPlayerInteractionInput input,
            IPlayerService player,
            ICommandDispatcher commandDispatcher,
            IInstanceRegistry<ITargetProvider> targetProviders,
            ISpawnedObjectRegistry spawnedObjects)
        {
            _input = input;
            _player = player;
            _commandDispatcher = commandDispatcher;
            _targetProviders = targetProviders;
            _spawnedObjects = spawnedObjects;
        }

        public void Start()
        {
            _input.InteractPerformed += InteractCurrentTarget;
        }

        public void Dispose()
        {
            _input.InteractPerformed -= InteractCurrentTarget;

            _interactionCts?.Cancel();
            _interactionCts?.Dispose();
            _interactionCts = null;
        }

        private void InteractCurrentTarget()
        {
            InteractCurrentTargetAsync().Forget();
        }

        private async UniTaskVoid InteractCurrentTargetAsync()
        {
            var currentActorId = _player.CurrentActor;

            if (currentActorId == Guid.Empty ||
                !_targetProviders.TryGet(
                    currentActorId,
                    out var targetProvider) ||
                !_spawnedObjects.TryGet(
                    currentActorId,
                    out var currentSpawnedObject))
            {
                return;
            }

            var target = targetProvider.CurrentTarget;

            if (target == null ||
                target.InstanceId == Guid.Empty)
            {
                return;
            }

            _interactionCts?.Cancel();
            _interactionCts?.Dispose();
            _interactionCts = new CancellationTokenSource();

            var command = new InteractCommand(
                currentActorId,
                currentSpawnedObject.GameObject.transform.position);

            await _commandDispatcher.SendAsync(
                target.InstanceId,
                command,
                _interactionCts.Token);
        }
    }
}