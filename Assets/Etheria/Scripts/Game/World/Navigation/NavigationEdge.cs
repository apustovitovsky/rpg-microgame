namespace Etheria.Game.World
{
    public readonly struct NavigationEdge
    {
        public string FromNodeId { get; }
        public string ToNodeId { get; }
        public float Cost { get; }
        public NavigationFlag Flags { get; }

        public NavigationEdge(
            string fromNodeId,
            string toNodeId,
            float cost,
            NavigationFlag flags = NavigationFlag.None)
        {
            FromNodeId = fromNodeId?.Trim() ?? string.Empty;
            ToNodeId = toNodeId?.Trim() ?? string.Empty;
            Cost = cost;
            Flags = flags;
        }
    }
}