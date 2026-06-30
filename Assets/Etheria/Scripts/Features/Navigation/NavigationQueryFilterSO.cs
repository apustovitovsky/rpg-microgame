using System;
using Etheria.Game.World;
using UnityEngine;

namespace Etheria.Navigation
{
    [CreateAssetMenu(
        menuName = "Etheria/Navigation/Query Filter",
        fileName = "NavigationQueryFilter")]
    public sealed class NavigationQueryFilterSO : ScriptableObject
    {
        [SerializeField] private Condition[] _node = Array.Empty<Condition>();
        [SerializeField] private Condition[] _edge = Array.Empty<Condition>();

        public NavigationQueryFilter ToFilter()
        {
            return new NavigationQueryFilter(
                BuildQuery(_node),
                BuildQuery(_edge));
        }

        private static NavigationFlagQuery BuildQuery(
            Condition[] conditions)
        {
            var query = NavigationFlagQuery.Any;

            if (conditions == null)
                return query;

            foreach (var condition in conditions)
            {
                if (condition == null)
                    continue;

                query = query.Merge(condition.ToQuery());
            }

            return query;
        }

        private enum Mode
        {
            Required,
            Excluded
        }

        [Serializable]
        private sealed class Condition
        {
            [SerializeField] private Mode _mode;
            [SerializeField] private NavigationFlag _flags;

            public NavigationFlagQuery ToQuery()
            {
                return _mode switch
                {
                    Mode.Required =>
                        new NavigationFlagQuery(
                            requiredFlags: _flags),

                    Mode.Excluded =>
                        new NavigationFlagQuery(
                            excludedFlags: _flags),

                    _ => NavigationFlagQuery.Any
                };
            }
        }
    }
}