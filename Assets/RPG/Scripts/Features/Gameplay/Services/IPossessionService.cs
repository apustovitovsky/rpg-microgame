namespace RPG.Gameplay
{
    public interface IPossessionService
    {
        void Possess(Actor actor);
        void Unpossess();
    }
}
