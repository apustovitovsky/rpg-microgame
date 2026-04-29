using Etheria.Game.Camera;
using Etheria.Game.Player;

namespace Etheria.Game.Input
{
    public interface IGameInputRouter
    {
        void SetHandler(IActorInputHandler handler);
        void RemoveHandler(IActorInputHandler handler);
        void SetHandler(IPlayerLookInputHandler handler);
        void RemoveHandler(IPlayerLookInputHandler handler);
        void SetHandler(ICameraInputHandler handler);
        void RemoveHandler(ICameraInputHandler handler);
    }
}

