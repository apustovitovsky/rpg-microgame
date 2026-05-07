namespace Etheria.Features.StoryletSystem
{
    public sealed class SetEntityAttributeEffect : EntityTargetedEffect
    {
        public SetEntityAttributeEffect(RoleId roleId, AttributeId attributeId, float value) : base(roleId)
        {
            AttributeId = attributeId;
            Value = value;
        }

        public AttributeId AttributeId { get; }
        public float Value { get; }
    }
}
