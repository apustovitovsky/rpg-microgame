namespace Etheria.Features.Collectables
{
    public interface IPickupInteractionHandler
    {
        bool TryCollect(IPickup pickup, IPickupTarget target);
    }
}

