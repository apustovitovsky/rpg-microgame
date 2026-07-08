namespace Game.World
{
    public interface IWorldIdFactory
    {
        WorldId Create(string prefix);
    }

    public sealed class WorldIdFactory : IWorldIdFactory
    {
        private int _nextIndex;

        public WorldId Create(string prefix)
        {
            _nextIndex++;

            return new WorldId(
                $"{NormalizePrefix(prefix)}_{_nextIndex:000}");
        }

        private static string NormalizePrefix(string value)
        {
            value = value?.Trim().ToLowerInvariant() ?? "world_object";

            if (string.IsNullOrWhiteSpace(value))
                return "world_object";

            var chars = value.ToCharArray();

            for (var i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]))
                    chars[i] = '_';
            }

            return new string(chars);
        }
    }
}