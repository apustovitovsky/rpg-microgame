

namespace RPG.Gameplay
{
    public interface IPickupCollector
    {
        string Name { get; }
        IInventory Inventory { get; }
        IHealth Health { get; }
    }
}