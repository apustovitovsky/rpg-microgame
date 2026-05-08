using System;
using UnityEngine;

namespace Etheria.Features.StoryletSystem.Authoring
{
    public enum ScoreBonusType : byte
    {
        Query = 0,
        Stack = 1
    }

    public enum StackScoreBonusMode : byte
    {
        Single = 0,
        PerStack = 1
    }

    [Serializable]
    public sealed class ScoreBonusAuthoring
    {
        [SerializeField]
        private ScoreBonusType _type;

        [SerializeField]
        private TagQueryAuthoring _query;

        [SerializeField]
        private string _tagId;

        [SerializeField]
        private StackScoreBonusMode _stackMode;

        [SerializeField]
        private float _bonusScore = 1f;

        [SerializeField]
        private float _weight = 1f;

        public ScoreBonusType Type => _type;
        public TagQueryAuthoring Query => _query;
        public string TagId => _tagId;
        public StackScoreBonusMode StackMode => _stackMode;
        public float BonusScore => _bonusScore;
        public float Weight => _weight;
    }
}