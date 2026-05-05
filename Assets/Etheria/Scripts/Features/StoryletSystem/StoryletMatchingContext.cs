using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletMatchingContext
    {
        private readonly List<Role> _allRoles = new();
        private readonly Dictionary<Role, List<Entity>> _compatibleEntitiesByRole = new();

        public StoryletMatchingContext(
            IReadOnlyList<Storylet> storylets,
            IReadOnlyList<Entity> entities)
        {
            if (storylets == null)
            {
                throw new ArgumentNullException(nameof(storylets));
            }

            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            foreach (var storylet in storylets)
            {
                if (storylet == null)
                {
                    continue;
                }

                foreach (var role in storylet.Roles)
                {
                    if (role == null || _compatibleEntitiesByRole.ContainsKey(role))
                    {
                        continue;
                    }

                    _allRoles.Add(role);

                    var compatibleEntities = new List<Entity>();

                    foreach (var entity in entities)
                    {
                        if (entity != null && entity.CanFill(role))
                        {
                            compatibleEntities.Add(entity);
                        }
                    }

                    _compatibleEntitiesByRole.Add(role, compatibleEntities);
                }
            }
        }

        public IReadOnlyList<Role> AllRoles => _allRoles;

        public float GetEntityVersatility(Entity entity)
        {
            var count = 0;

            foreach (var role in _allRoles)
            {
                if (entity.CanFill(role))
                {
                    count++;
                }
            }

            return count;
        }

        public IReadOnlyList<Entity> GetCompatibleEntities(Role role)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return _compatibleEntitiesByRole.TryGetValue(role, out var compatibleEntities)
                ? compatibleEntities
                : Array.Empty<Entity>();
        }

        public int CountCompatibleEntities(
            Role role,
            HashSet<Entity> freeEntities)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (freeEntities == null)
            {
                throw new ArgumentNullException(nameof(freeEntities));
            }

            var count = 0;
            var compatibleEntities = GetCompatibleEntities(role);

            foreach (var entity in compatibleEntities)
            {
                if (freeEntities.Contains(entity))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
