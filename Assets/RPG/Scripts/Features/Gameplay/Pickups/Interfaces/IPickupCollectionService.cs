namespace RPG.Gameplay
{
    public interface IPickupInteractionService
    {
        bool TryCollect(IPickupInstance pickup, IPickupCollector collector);
    }
}
