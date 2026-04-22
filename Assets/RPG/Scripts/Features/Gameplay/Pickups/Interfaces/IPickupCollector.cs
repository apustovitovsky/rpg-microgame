

namespace RPG.Gameplay
{
    public interface IPickupCollector
    {
        string Name { get; }
        bool TryGet<T>(out T service) where T : class;
    }
}
