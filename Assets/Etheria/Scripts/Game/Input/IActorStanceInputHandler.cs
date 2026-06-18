namespace Etheria.Game.Input
{
    public interface IActorStanceInputHandler
    {
        void HandleSprint(bool isPressed);
        void HandleCrouch(bool isPressed);
        void HandleAim(bool isPressed);
        void HandleLockOn();
        void HandleWalkToggle();
    }
}
