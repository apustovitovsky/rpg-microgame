namespace RPG.Gameplay
{
    public interface IPickupInstance
    {
        PickupDefinitionSO Definition { get; }
        bool TryCollect(IPickupCollector collector);
    }
}
