using System;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletRejectionReason
    {
        public StoryletRejectionReason(string category, string message)
        {
            Category = category ?? throw new ArgumentNullException(nameof(category));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public string Category { get; }
        public string Message { get; }

        public override string ToString()
        {
            return $"{Category}: {Message}";
        }
    }
}
