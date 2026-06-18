namespace Etheria.Features.Collectables
{
    public interface IPickup
    {
        bool TryApplyTo(IPickupTarget target);
    }
}

