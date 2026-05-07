namespace Etheria.Features.StoryletSystem
{
    public readonly struct TagQuery
    {
        public static readonly TagQuery Empty = new(
            TagSet.Empty,
            TagSet.Empty,
            TagSet.Empty);

        public readonly TagSet AllOf;
        public readonly TagSet AnyOf;
        public readonly TagSet NoneOf;

        public TagQuery(TagSet allOf, TagSet anyOf, TagSet noneOf)
        {
            AllOf = allOf;
            AnyOf = anyOf;
            NoneOf = noneOf;
        }

        public bool IsEmpty =>
            AllOf.IsEmpty &&
            AnyOf.IsEmpty &&
            NoneOf.IsEmpty;

        public readonly bool Matches(TagSet tags)
        {
            if (!tags.ContainsAll(AllOf))
            {
                return false;
            }

            if (!AnyOf.IsEmpty && !tags.Overlaps(AnyOf))
            {
                return false;
            }

            if (!tags.Excludes(NoneOf))
            {
                return false;
            }

            return true;
        }
    }
}