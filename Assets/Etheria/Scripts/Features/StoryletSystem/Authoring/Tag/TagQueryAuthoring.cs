using System;
using System.Collections.Generic;
using UnityEngine;

namespace Etheria.Features.StoryletSystem.Authoring
{
    [Serializable]
    public struct TagQueryAuthoring
    {
        [SerializeField]
        private TagQueryEntryAuthoring[] _entries;

        public readonly IReadOnlyList<TagQueryEntryAuthoring> Entries => _entries;
    }

    [Serializable]
    public struct TagQueryEntryAuthoring
    {
        [SerializeField]
        private TagQueryType _type;

        [SerializeField]
        private string[] _tagIds;

        public readonly TagQueryType Type => _type;
        public readonly IReadOnlyList<string> TagIds => _tagIds;
    }
}
