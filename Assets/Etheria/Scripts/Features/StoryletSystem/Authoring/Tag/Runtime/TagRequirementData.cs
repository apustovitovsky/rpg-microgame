namespace Etheria.Features.StoryletSystem.Authoring
{
    public enum TagRequirementType : byte
    {
        Query = 0,
        Stack = 1
    }

    public readonly struct TagRequirementData
    {
        private readonly TagRequirementType _type;
        private readonly TagQuery _query;
        private readonly TagId _tagId;
        private readonly TagStackQueryType _stackQueryType;
        private readonly ushort _stackValue;

        public TagRequirementData(TagQuery query)
        {
            _type = TagRequirementType.Query;
            _query = query;

            _tagId = default;
            _stackQueryType = default;
            _stackValue = 0;
        }

        public TagRequirementData(TagId tagId, TagStackQueryType stackQueryType, ushort stackValue)
        {
            _type = TagRequirementType.Stack;

            _query = default;

            _tagId = tagId;
            _stackQueryType = stackQueryType;
            _stackValue = stackValue;
        }

        public bool Matches(TagSet tags)
        {
            return _type switch
            {
                TagRequirementType.Query => tags.Matches(_query),
                TagRequirementType.Stack => MatchesStack(tags.GetStack(_tagId)),
                _ => false
            };
        }

        private bool MatchesStack(ushort actual)
        {
            return TagCompare.Matches(actual, _stackQueryType, _stackValue);
        }
    }
}
