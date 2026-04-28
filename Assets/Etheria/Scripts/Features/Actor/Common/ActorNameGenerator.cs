namespace Etheria.Features.Actor
{
    public interface INameGenerator
    {
        string Generate(string prefix = "");
    }

    public sealed class ActorNameGenerator : INameGenerator
    {
        private int _counter;

        public string Generate(string prefix = "")
        {
            _counter++;

            return string.IsNullOrWhiteSpace(prefix)
                ? _counter.ToString("000")
                : $"{prefix}_{_counter:000}";
        }
    }
}
