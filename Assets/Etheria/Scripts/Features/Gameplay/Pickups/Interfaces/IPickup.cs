namespace Etheria.Features.Gameplay
{
    public interface IPickup
    {
        bool TryApplyTo(IPickupTarget target);
    }
}

