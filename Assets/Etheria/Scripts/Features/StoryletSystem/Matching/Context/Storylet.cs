using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class Storylet
    {
        public Storylet(
            StoryletId id,
            string key,
            float priority,
            IReadOnlyList<Role> roles)
        {
            Id = id;
            Key = key;
            Priority = priority;
            Roles = roles ?? throw new ArgumentNullException(nameof(roles));
        }

        public StoryletId Id { get; }
        public string Key { get; }
        public float Priority { get; }
        public IReadOnlyList<Role> Roles { get; }

        public override string ToString()
        {
            return Key;
        }
    }
}
