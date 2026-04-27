namespace Etheria.Gameplay
{
    public interface IPickup
    {
        bool TryApplyTo(IPickupTarget target);
    }
}
