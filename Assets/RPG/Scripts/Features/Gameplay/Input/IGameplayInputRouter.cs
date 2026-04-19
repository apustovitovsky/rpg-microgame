namespace RPG.Gameplay
{
    public interface IGameplayInputRouter
    {
        void SetHandler(IPlayerInputHandler handler);
        void RemoveHandler(IPlayerInputHandler handler);
        void SetHandler(ICameraInputHandler handler);
        void RemoveHandler(ICameraInputHandler handler);
    }
}
