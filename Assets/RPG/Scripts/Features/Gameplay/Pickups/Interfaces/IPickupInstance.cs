namespace RPG.Gameplay
{
    public interface IPickupInstance
    {
        string DisplayName { get; }
        bool TryCollect(IPickupCollector collector);
    }
}
