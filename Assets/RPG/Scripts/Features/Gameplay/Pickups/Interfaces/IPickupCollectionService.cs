namespace RPG.Gameplay
{
    public interface IPickupInteractionHandler
    {
        bool TryCollect(IPickupInstance pickup, IPickupCollector collector);
    }
}
