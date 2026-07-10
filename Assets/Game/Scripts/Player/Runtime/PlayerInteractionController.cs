using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Actor;
using Game.Interaction;
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
        private readonly IActorService _actors;

        private CancellationTokenSource _interactionCts;

        public PlayerInteractionController(
            IPlayerInteractionInput input,
            IPlayerService player,
            IInteractionService interactions,
            IActorService actors)
        {
            _input = input;
            _player = player;
            _interactions = interactions;
            _actors = actors;
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
                !_actors.TryGet(
                    currentActorId,
                    out var currentActor) ||
                currentActor.View == null ||
                currentActor.Targeting == null)
            {
                return;
            }

            var target = currentActor.Targeting.CurrentTarget;

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
                currentActor.View.Root.position,
                target.InstanceId);

            await _interactions.TryInteractAsync(
                context,
                _interactionCts.Token);
        }
    }
}