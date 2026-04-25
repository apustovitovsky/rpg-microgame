namespace RPG.Gameplay
{
    public interface IPickupInteractionHandler
    {
        bool TryCollect(IPickup pickup, IPickupTarget target);
    }
}
