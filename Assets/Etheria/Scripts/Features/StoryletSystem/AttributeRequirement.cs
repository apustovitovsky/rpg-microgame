namespace Etheria.Features.StoryletSystem
{
    public readonly struct AttributeRequirement
    {
        public AttributeRequirement(
            AttributeId attributeId,
            float minValue = 0f,
            bool hasMinValue = false,
            float maxValue = 0f,
            bool hasMaxValue = false)
        {
            AttributeId = attributeId;
            MinValue = minValue;
            HasMinValue = hasMinValue;
            MaxValue = maxValue;
            HasMaxValue = hasMaxValue;
        }

        public AttributeId AttributeId { get; }
        public float MinValue { get; }
        public bool HasMinValue { get; }
        public float MaxValue { get; }
        public bool HasMaxValue { get; }

        public static AttributeRequirement Min(AttributeId attributeId, float minValue)
        {
            return new AttributeRequirement(
                attributeId,
                minValue: minValue,
                hasMinValue: true);
        }

        public static AttributeRequirement Max(AttributeId attributeId, float maxValue)
        {
            return new AttributeRequirement(
                attributeId,
                maxValue: maxValue,
                hasMaxValue: true);
        }
    }
}
