
namespace Etheria.Features.HWFC
{
    public readonly struct EntropyQueueEntry
    {
        public readonly Slot Slot;
        public readonly int Version;

        public EntropyQueueEntry(Slot slot)
        {
            Slot = slot;
            Version = slot.Version;
        }
    }
}