using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;
using UnityEngine;

namespace Game.Interaction
{
    public sealed class InteractionService : IInteractionService
    {
        private readonly IWorldRegistry<IWorldObject> _worldObjects;
        private readonly IWorldRegistry<IInteractable> _interactions;

        public InteractionService(
            IWorldRegistry<IWorldObject> worldObjects,
            IWorldRegistry<IInteractable> interactions)
        {
            _worldObjects = worldObjects;
            _interactions = interactions;
        }

        public async UniTask<bool> TryInteractAsync(
            IWorldObject interactor,
            WorldId targetWorldId,
            CancellationToken token)
        {
            if (interactor == null ||
                targetWorldId.IsEmpty)
            {
                return false;
            }

            if (!_worldObjects.TryGet(targetWorldId, out var target))
                return false;

            if (!_interactions.TryGet(targetWorldId, out var interactable))
                return false;

            if (Vector3.Distance(
                    interactor.Root.position,
                    target.Root.position) > interactable.MaxRange)
            {
                return false;
            }

            var context = new InteractionContext(
                interactor,
                target);

            if (!interactable.CanInteract(context))
                return false;

            await interactable.InteractAsync(
                context,
                token);

            return true;
        }
    }
}