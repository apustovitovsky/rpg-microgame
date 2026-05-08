namespace Etheria.Features.StoryletSystem.Authoring
{
    public enum TagQueryType : byte
    {
        Required = 0,
        Any = 1,
        Excluded = 2,
    }

    public readonly struct TagQuery
    {
        private readonly TagQueryEntry[] _entries;

        public TagQuery(TagQueryEntry[] entries)
        {
            _entries = entries;
        }

        public bool Matches(TagMask tags)
        {
            if (_entries == null)
            {
                return true;
            }

            for (var i = 0; i < _entries.Length; i++)
            {
                if (!_entries[i].Matches(tags))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public readonly struct TagQueryEntry
    {
        private readonly TagQueryType _type;
        private readonly TagMask _tags;

        public TagQueryEntry(TagQueryType type, TagMask tags)
        {
            _type = type;
            _tags = tags;
        }

        public bool Matches(TagMask tags)
        {
            return _type switch
            {
                TagQueryType.Required => tags.ContainsAll(_tags),
                TagQueryType.Any => _tags.IsEmpty || tags.Overlaps(_tags),
                TagQueryType.Excluded => tags.Excludes(_tags),
                _ => false
            };
        }
    }
}