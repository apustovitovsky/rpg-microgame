namespace RPG.Gameplay
{
    public interface IPickupInstance
    {
        string DisplayName { get; }
        WorldPickup WorldPickup { get; }
        bool TryCollect(IPickupCollector collector);
    }
}
