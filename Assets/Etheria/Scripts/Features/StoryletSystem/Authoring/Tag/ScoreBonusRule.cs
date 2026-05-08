
using RuntimeTagQuery = Etheria.Features.StoryletSystem.Authoring.TagQuery;

namespace Etheria.Features.StoryletSystem.Authoring
{
    public readonly struct ScoreBonusRule
    {
        private readonly ScoreBonusType _type;

        private readonly RuntimeTagQuery _query;

        private readonly TagId _tagId;
        private readonly StackScoreBonusMode _stackMode;

        private readonly float _bonusScore;
        private readonly float _weight;

        public ScoreBonusRule(
            RuntimeTagQuery query,
            float bonusScore,
            float weight)
        {
            _type = ScoreBonusType.Query;
            _query = query;

            _tagId = default;
            _stackMode = default;

            _bonusScore = bonusScore;
            _weight = weight;
        }

        public ScoreBonusRule(
            TagId tagId,
            StackScoreBonusMode stackMode,
            float bonusScore,
            float weight)
        {
            _type = ScoreBonusType.Stack;
            _query = default;

            _tagId = tagId;
            _stackMode = stackMode;

            _bonusScore = bonusScore;
            _weight = weight;
        }

        public bool TryEvaluate(
            TagSet tags,
            out float weightedScore,
            out float weight)
        {
            weightedScore = 0f;
            weight = 0f;

            var rawScore = _type switch
            {
                ScoreBonusType.Query => EvaluateQuery(tags),
                ScoreBonusType.Stack => EvaluateStack(tags),
                _ => 0f
            };

            if (rawScore == 0f)
            {
                return false;
            }

            weightedScore = rawScore * _weight;
            weight = _weight;
            return true;
        }

        private float EvaluateQuery(TagSet tags)
        {
            return tags.Matches(_query)
                ? _bonusScore
                : 0f;
        }

        private float EvaluateStack(TagSet tags)
        {
            var stack = tags.GetStack(_tagId);

            return _stackMode switch
            {
                StackScoreBonusMode.Single => stack > 0
                    ? _bonusScore
                    : 0f,

                StackScoreBonusMode.PerStack => _bonusScore * stack,

                _ => 0f
            };
        }
    }
}
