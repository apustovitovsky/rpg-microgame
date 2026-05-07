using System;

namespace Etheria.Features.StoryletSystem
{
    public sealed class RoleAssignment
    {
        public RoleAssignment(Role role, Entity entity)
        {
            Role = role ?? throw new ArgumentNullException(nameof(role));
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        }

        public Role Role { get; }
        public Entity Entity { get; }
    }
}