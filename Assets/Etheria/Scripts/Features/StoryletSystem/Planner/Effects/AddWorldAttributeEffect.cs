namespace Etheria.Features.StoryletSystem
{
    public sealed class AddWorldAttributeEffect : StoryletEffect
    {
        public AddWorldAttributeEffect(AttributeId attributeId, float delta)
        {
            AttributeId = attributeId;
            Delta = delta;
        }

        public AttributeId AttributeId { get; }
        public float Delta { get; }
    }
}
