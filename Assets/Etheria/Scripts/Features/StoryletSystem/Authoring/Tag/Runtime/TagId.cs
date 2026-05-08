namespace Etheria.Features.StoryletSystem.Authoring
{
    public readonly struct TagId
    {
        public readonly int Index;

        public TagId(int index)
        {
            Index = index;
        }

        public bool IsValid => Index >= 0;
    }
}