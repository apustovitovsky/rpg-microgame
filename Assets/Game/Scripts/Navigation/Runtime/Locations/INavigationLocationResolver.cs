namespace Game.Navigation
{
    public interface INavigationLocationResolver
    {
        bool TryResolveAnchorNodeId(
            string locationId,
            string anchorKey,
            out string nodeId);

        bool TryResolveDefaultAnchorNodeId(
            string locationId,
            out string nodeId);
    }
}