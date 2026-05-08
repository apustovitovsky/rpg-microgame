using System;

namespace Etheria.Features.StoryletSystem
{
    internal static class StoryletIdRange
    {
        public const int MinValue = 0;
        public const int MaxValue = TagSet.Capacity - 1;

        public static int EnsureValid(string typeName, int value)
        {
            if (value < MinValue || value > MaxValue)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    $"{typeName} must be in range {MinValue}..{MaxValue}.");
            }

            return value;
        }
    }
}
