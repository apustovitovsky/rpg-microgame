namespace RPG.Gameplay
{
    public interface IPickupInstance
    {
        string DisplayName { get; }
        bool IsCollected { get; }
        bool TryCollect(IPickupCollector collector);
    }
}
