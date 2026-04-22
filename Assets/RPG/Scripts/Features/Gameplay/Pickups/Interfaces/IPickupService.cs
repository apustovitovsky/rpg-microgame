namespace RPG.Gameplay
{
    public interface IPickupService
    {
        bool TryCollect(IPickup pickup, IPickupCollector collector);
    }
}
