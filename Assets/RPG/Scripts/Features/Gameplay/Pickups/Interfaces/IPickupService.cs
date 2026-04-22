namespace RPG.Gameplay
{
    public interface IPickupService
    {
        bool TryCollect(IPickupInstance pickup, IPickupCollector collector);
    }
}
