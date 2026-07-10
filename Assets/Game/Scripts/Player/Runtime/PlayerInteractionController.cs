using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        private readonly IInteractionService _interactions;
        private readonly IInstanceRegistry<ITargetProvider> _targetProviders;
        private readonly ISpawnedObjectRegistry _spawnedObjects;

        private CancellationTokenSource _interactionCts;

        public PlayerInteractionController(
            IPlayerInteractionInput input,
            IPlayerService player,
            IInteractionService interactions,
            IInstanceRegistry<ITargetProvider> targetProviders,
            ISpawnedObjectRegistry spawnedObjects)
        {
            _input = input;
            _player = player;
            _interactions = interactions;
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

            var context = new InteractionContext(
                currentActorId,
                currentSpawnedObject.GameObject.transform.position,
                target.InstanceId);

            await _interactions.TryInteractAsync(
                context,
                _interactionCts.Token);
        }
    }
}