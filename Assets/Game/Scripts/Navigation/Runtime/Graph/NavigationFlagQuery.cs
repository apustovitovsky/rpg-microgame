namespace Game.Navigation
{
    public readonly struct NavigationFlagQuery
    {
        public NavigationFlagQuery(
            NavigationFlag requiredFlags =
                NavigationFlag.None,
            NavigationFlag excludedFlags =
                NavigationFlag.None)
        {
            RequiredFlags = requiredFlags;
            ExcludedFlags = excludedFlags;
        }

        public NavigationFlag RequiredFlags { get; }

        public NavigationFlag ExcludedFlags { get; }

        public bool Matches(
            NavigationFlag flags)
        {
            if ((flags & RequiredFlags) != RequiredFlags)
                return false;

            return (flags & ExcludedFlags) == 0;
        }

        public NavigationFlagQuery Merge(
            NavigationFlagQuery other)
        {
            return new NavigationFlagQuery(
                RequiredFlags | other.RequiredFlags,
                ExcludedFlags | other.ExcludedFlags);
        }

        public static NavigationFlagQuery Any =>
            new();
    }
}