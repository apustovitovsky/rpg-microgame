using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;
using UnityEngine;

namespace Game.Interaction
{
    public sealed class InteractionService : IInteractionService
    {
        private readonly IWorldRegistry<IWorldSpatial> _spatials;
        private readonly IWorldRegistry<IInteractable> _interactions;

        public InteractionService(
            IWorldRegistry<IWorldSpatial> spatials,
            IWorldRegistry<IInteractable> interactions)
        {
            _spatials = spatials;
            _interactions = interactions;
        }

        public async UniTask<bool> TryInteractAsync(
            IWorldHandle interactor,
            WorldId targetWorldId,
            CancellationToken token)
        {
            if (interactor == null ||
                targetWorldId.IsEmpty)
            {
                return false;
            }

            if (!_spatials.TryGet(interactor.WorldId, out var interactorSpatial))
                return false;

            if (!_spatials.TryGet(targetWorldId, out var targetSpatial))
                return false;

            if (!_interactions.TryGet(targetWorldId, out var interactable))
                return false;

            if (Vector3.Distance(
                    interactorSpatial.Position,
                    targetSpatial.Position) > interactable.MaxRange)
            {
                return false;
            }

            var context = new InteractionContext(
                interactor,
                targetWorldId);

            if (!interactable.CanInteract(context))
                return false;

            await interactable.InteractAsync(
                context,
                token);

            return true;
        }
    }
}