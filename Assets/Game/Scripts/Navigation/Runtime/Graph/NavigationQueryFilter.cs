namespace Game.Navigation
{
    public readonly struct NavigationQueryFilter
    {
        public NavigationQueryFilter(
            NavigationFlagQuery nodeQuery,
            NavigationFlagQuery edgeQuery)
        {
            NodeQuery = nodeQuery;
            EdgeQuery = edgeQuery;
        }

        public NavigationFlagQuery NodeQuery { get; }

        public NavigationFlagQuery EdgeQuery { get; }

        public bool IsNodeAllowed(
            NavigationNode node)
        {
            return node != null &&
                   NodeQuery.Matches(node.Flags);
        }

        public bool IsEdgeAllowed(
            NavigationEdge edge)
        {
            return EdgeQuery.Matches(edge.Flags);
        }

        public static NavigationQueryFilter Any =>
            new(
                NavigationFlagQuery.Any,
                NavigationFlagQuery.Any);
    }
}