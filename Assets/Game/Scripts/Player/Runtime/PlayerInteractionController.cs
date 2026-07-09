using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Actor;
using Game.Interaction;
using Game.Targeting;
using Game.World;
using UnityEngine;
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
        private readonly IWorldRegistry<IWorldActor> _actors;
        private readonly IWorldRegistry<ITargetProvider> _targetProviders;

        private CancellationTokenSource _interactionCts;

        public PlayerInteractionController(
            IPlayerInteractionInput input,
            IPlayerService player,
            IInteractionService interactions,
            IWorldRegistry<IWorldActor> actors,
            IWorldRegistry<ITargetProvider> targetProviders)
        {
            _input = input;
            _player = player;
            _interactions = interactions;
            _actors = actors;
            _targetProviders = targetProviders;
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

            if (currentActorId.IsEmpty ||
                !_actors.TryGet(currentActorId, out var currentActor) ||
                currentActor.View == null ||
                !_targetProviders.TryGet(currentActorId, out var targetProvider))
            {
                return;
            }

            var target = targetProvider.CurrentTarget;

            if (target == null ||
                target.WorldId.IsEmpty)
            {
                return;
            }

            _interactionCts?.Cancel();
            _interactionCts?.Dispose();
            _interactionCts = new CancellationTokenSource();

            var result = await _interactions.TryInteractAsync(
                currentActorId,
                currentActor.View.Root.position,
                target.WorldId,
                _interactionCts.Token);

            if (result != InteractionResult.Succeeded)
            {
                Debug.Log(
                    $"Interaction with '{target.WorldId}' failed: {result}.",
                    target.Root);
            }
        }
    }
}