namespace Etheria.Features.StoryletSystem
{
    public sealed class AddEntityAttributeEffect : EntityTargetedEffect
    {
        public AddEntityAttributeEffect(RoleId roleId, AttributeId attributeId, float delta) : base(roleId)
        {
            AttributeId = attributeId;
            Delta = delta;
        }

        public AttributeId AttributeId { get; }
        public float Delta { get; }
    }
}
