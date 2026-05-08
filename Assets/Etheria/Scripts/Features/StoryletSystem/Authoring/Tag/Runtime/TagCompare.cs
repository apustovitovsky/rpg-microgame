namespace Etheria.Features.StoryletSystem.Authoring
{
    internal static class TagCompare
    {
        public static bool Matches(ushort actual, TagStackQueryType comparison, ushort expected)
        {
            return comparison switch
            {
                TagStackQueryType.Equal => actual == expected,
                TagStackQueryType.NotEqual => actual != expected,
                TagStackQueryType.More => actual > expected,
                TagStackQueryType.MoreOrEqual => actual >= expected,
                TagStackQueryType.Less => actual < expected,
                TagStackQueryType.LessOrEqual => actual <= expected,
                _ => false
            };
        }
    }
}
