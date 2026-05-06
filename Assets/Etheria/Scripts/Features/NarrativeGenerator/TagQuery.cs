namespace Etheria.Features.NarrativeGeneration
{
    public struct TagQuery
    {
        public TagSet AllOf;
        public TagSet AnyOf;
        public TagSet NoneOf;

        public readonly bool Matches(TagSet tags)
        {
            if (AllOf != TagSet.None && !tags.ContainsAll(AllOf))
            {
                return false;
            }

            if (AnyOf != TagSet.None && !tags.ContainsAny(AnyOf))
            {
                return false;
            }

            if (NoneOf != TagSet.None && tags.ContainsAny(NoneOf))
            {
                return false;
            }

            return true;
        }
    }
}
