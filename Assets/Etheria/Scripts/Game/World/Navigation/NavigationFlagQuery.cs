namespace Etheria.Game.World
{
    public readonly struct NavigationFlagQuery
    {
        public NavigationFlag RequiredFlags { get; }
        public NavigationFlag ExcludedFlags { get; }

        public NavigationFlagQuery(
            NavigationFlag requiredFlags = NavigationFlag.None,
            NavigationFlag excludedFlags = NavigationFlag.None)
        {
            RequiredFlags = requiredFlags;
            ExcludedFlags = excludedFlags;
        }

        public bool Matches(NavigationFlag flags)
        {
            if ((flags & RequiredFlags) != RequiredFlags)
                return false;

            if ((flags & ExcludedFlags) != 0)
                return false;

            return true;
        }

        public NavigationFlagQuery Merge(
            NavigationFlagQuery other)
        {
            return new NavigationFlagQuery(
                RequiredFlags | other.RequiredFlags,
                ExcludedFlags | other.ExcludedFlags);
        }

        public static NavigationFlagQuery Any => new();
    }
}