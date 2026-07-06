using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Interaction
{
    public sealed class InteractionService : IInteractionService
    {
        private readonly IWorldObjectRegistry _worldObjects;

        public InteractionService(IWorldObjectRegistry worldObjects)
        {
            _worldObjects = worldObjects;
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

            if (!_worldObjects.TryGet(
                    targetWorldId,
                    out var target))
            {
                return false;
            }

            if (!target.TryGet<IInteractable>(
                    out var interactable))
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