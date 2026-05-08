using System;
using System.Collections.Generic;
using UnityEngine;

namespace Etheria.Features.StoryletSystem.Authoring
{
    [Serializable]
    public struct TagCollectionAuthoring
    {
        [SerializeField]
        private string[] _tagIds;

        public readonly IReadOnlyList<string> TagIds => _tagIds;
    }
}
