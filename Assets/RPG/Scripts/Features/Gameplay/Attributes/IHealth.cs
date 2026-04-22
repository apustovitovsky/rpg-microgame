namespace RPG.Gameplay
{
    public interface IHealth
    {
        float CurHealth { get; }
        float MaxHealth { get; }
        bool IsFull { get; }
        void Heal(float amount);
    }
}