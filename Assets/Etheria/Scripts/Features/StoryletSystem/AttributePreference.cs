namespace Etheria.Features.StoryletSystem
{
    public readonly struct AttributePreference
    {
        public AttributePreference(
            AttributeId attributeId,
            float weight,
            float start,
            float end)
        {
            AttributeId = attributeId;
            Weight = weight;
            Start = start;
            End = end;
        }

        public AttributeId AttributeId { get; }
        public float Weight { get; }
        public float Start { get; }
        public float End { get; }
    }
}
