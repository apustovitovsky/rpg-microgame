namespace Game.Navigation
{
    public readonly struct NavigationEdge
    {
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

        public string FromNodeId { get; }

        public string ToNodeId { get; }

        public float Cost { get; }

        public NavigationFlag Flags { get; }
    }
}