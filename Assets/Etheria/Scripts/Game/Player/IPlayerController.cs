using VContainer.Unity;

namespace Etheria.Game.Player
{
    public interface IPlayerController
    {
        void Possess(LifetimeScope actor);
        void Unpossess();
    }
}

