namespace Etheria.Features.HWFC
{
    public readonly struct EntropyQueueEntry
    {
        public readonly int NodeIndex;
        public readonly int Version;
        public readonly float Entropy;

        public EntropyQueueEntry(int nodeIndex, int version, float entropy)
        {
            NodeIndex = nodeIndex;
            Version = version;
            Entropy = entropy;
        }
    }
}
