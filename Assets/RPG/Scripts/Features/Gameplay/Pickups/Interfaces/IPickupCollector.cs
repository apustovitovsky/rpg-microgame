

namespace RPG.Gameplay
{
    public interface IPickupCollector
    {
        string DisplayName { get; }
        bool TryGet<T>(out T service) where T : class;
    }
}
