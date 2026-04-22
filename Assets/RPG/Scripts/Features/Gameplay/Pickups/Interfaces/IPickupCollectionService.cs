namespace RPG.Gameplay
{
    public interface IPickupCollectionService
    {
        bool TryCollect(IPickupInstance pickup, IPickupCollector collector);
    }
}
