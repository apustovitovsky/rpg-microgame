namespace Etheria.Gameplay
{
    public interface IActorNamingService
    {
        string Generate(string prefix = "Actor");
    }

    public sealed class ActorNamingService : IActorNamingService
    {
        private int _counter;

        public string Generate(string prefix = "Actor")
        {
            _counter++;
            return $"{prefix}_{_counter:000}";
        }
    }
}