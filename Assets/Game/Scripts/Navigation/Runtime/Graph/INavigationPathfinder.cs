namespace Game.Navigation
{
    public interface INavigationPathfinder
    {
        NavigationPath FindPath(
            NavigationGraph graph,
            string fromNodeId,
            string toNodeId);

        NavigationPath FindPath(
            NavigationGraph graph,
            string fromNodeId,
            string toNodeId,
            NavigationQueryFilter filter);

        bool TryFindPath(
            NavigationGraph graph,
            string fromNodeId,
            string toNodeId,
            NavigationQueryFilter filter,
            out NavigationPath path);
    }
}