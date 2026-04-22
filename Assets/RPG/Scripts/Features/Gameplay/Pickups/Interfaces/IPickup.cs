
namespace RPG.Gameplay
{
    public interface IPickup
    {
        bool IsCollected { get; }
        bool TryCollect(IPickupCollector other);
    }
}
