namespace RPG.Gameplay
{
    public interface IPickup
    {
        bool TryApplyTo(IPickupTarget target);
    }
}
