namespace RPG.Gameplay
{
    public interface IPickupEffect
    {
        bool TryApply(IPickupCollector collector);
    }
}
