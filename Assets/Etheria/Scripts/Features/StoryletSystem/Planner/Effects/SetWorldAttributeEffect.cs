namespace Etheria.Features.StoryletSystem
{
    public sealed class SetWorldAttributeEffect : StoryletEffect
    {
        public SetWorldAttributeEffect(AttributeId attributeId, float value)
        {
            AttributeId = attributeId;
            Value = value;
        }

        public AttributeId AttributeId { get; }
        public float Value { get; }
    }
}
