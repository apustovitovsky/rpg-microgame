using System;
using UnityEngine;

namespace Etheria.Features.StoryletSystem.Authoring
{
    public enum TagStackQueryType : byte
    {
        Equal = 0,
        NotEqual = 1,

        More = 2,
        MoreOrEqual = 3,

        Less = 4,
        LessOrEqual = 5
    }

    [Serializable]
    public struct TagStackQueryAuthoring
    {
        [SerializeField]
        private string _tagId;

        [SerializeField]
        private TagStackQueryType _queryType;

        [SerializeField]
        private ushort _value;

        public readonly string TagId => _tagId;
        public readonly TagStackQueryType QueryType => _queryType;
        public readonly ushort Value => _value;
    }
}