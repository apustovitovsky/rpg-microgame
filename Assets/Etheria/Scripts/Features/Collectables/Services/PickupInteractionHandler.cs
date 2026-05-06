using System.Collections.Generic;

namespace Etheria.Features.Collectables
{
    public sealed class PickupInteractionHandler : IPickupInteractionHandler
    {
        private readonly HashSet<IPickup> _inProgress = new();

        public bool TryCollect(IPickup pickup, IPickupTarget target)
        {
            if (pickup == null || target == null)
                return false;

            if (!_inProgress.Add(pickup))
                return false;

            try
            {
                if (!pickup.TryApplyTo(target))
                    return false;

                return true;
            }
            finally
            {
                _inProgress.Remove(pickup);
            }
        }
    }
}

