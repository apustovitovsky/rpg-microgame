namespace Etheria.Features.StoryletSystem
{
    public readonly struct TagQuery
    {
        public static readonly TagQuery Empty = new(
            TagSet.Empty,
            TagSet.Empty,
            TagSet.Empty);

        public readonly TagSet Required;
        public readonly TagSet Any;
        public readonly TagSet Excluded;

        public TagQuery(TagSet required, TagSet any, TagSet excluded)
        {
            Required = required;
            Any = any;
            Excluded = excluded;
        }

        public bool IsEmpty =>
            Required.IsEmpty &&
            Any.IsEmpty &&
            Excluded.IsEmpty;

        public readonly bool Matches(TagSet tags)
        {
            if (!tags.ContainsAll(Required))
            {
                return false;
            }

            if (!Any.IsEmpty && !tags.Overlaps(Any))
            {
                return false;
            }

            if (!tags.Excludes(Excluded))
            {
                return false;
            }

            return true;
        }
    }
}