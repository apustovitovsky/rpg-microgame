using Etheria.Features.Camera;
using Etheria.Game.Camera;
using Etheria.Game.Player;

namespace Etheria.Features.Input
{
    public interface IGameplayInputRouter
    {
        void SetHandler(IActorInputHandler handler);
        void RemoveHandler(IActorInputHandler handler);
        void SetHandler(IPlayerLookInputHandler handler);
        void RemoveHandler(IPlayerLookInputHandler handler);
        void SetHandler(ICameraInputHandler handler);
        void RemoveHandler(ICameraInputHandler handler);
    }
}

