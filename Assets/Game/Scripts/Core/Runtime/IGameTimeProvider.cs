namespace Game.Core
{
    public interface IGameTimeProvider
    {
        float DeltaTime { get; }
    }
}