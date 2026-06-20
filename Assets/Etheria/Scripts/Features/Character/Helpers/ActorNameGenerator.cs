using Etheria.Game.Common;

namespace Etheria.Features.Character
{
    public sealed class ActorNameGenerator : IPrefixedStringGenerator
    {
        private int _counter;

        public string Generate(string prefix = "Actor")
        {
            _counter++;

            return string.IsNullOrWhiteSpace(prefix)
                ? _counter.ToString("000")
                : $"{prefix}_{_counter:000}";
        }
    }
}
