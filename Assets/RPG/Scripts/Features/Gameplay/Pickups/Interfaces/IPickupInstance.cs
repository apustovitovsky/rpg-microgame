namespace RPG.Gameplay
{
    public interface IPickupInstance
    {
        bool IsCollected { get; }
        bool TryCollect(IPickupCollector collector);
    }
}
