using VContainer.Unity;

namespace RPG.Gameplay
{
    public interface IPossessionService
    {
        void Possess(LifetimeScope scope);
        void Unpossess();
    }
}
