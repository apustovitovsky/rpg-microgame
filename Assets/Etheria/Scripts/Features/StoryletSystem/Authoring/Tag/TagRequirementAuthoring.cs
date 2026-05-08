using System;
using UnityEngine;

namespace Etheria.Features.StoryletSystem.Authoring
{
    [Serializable]
    public struct TagRequirementAuthoring
    {
        [SerializeField]
        private TagRequirementType _type;

        [SerializeField]
        private TagQueryAuthoring _query;

        [SerializeField]
        private TagStackQueryAuthoring _stack;

        public readonly TagRequirementType Type => _type;
        public readonly TagQueryAuthoring Query => _query;
        public readonly TagStackQueryAuthoring Stack => _stack;
    }
}
