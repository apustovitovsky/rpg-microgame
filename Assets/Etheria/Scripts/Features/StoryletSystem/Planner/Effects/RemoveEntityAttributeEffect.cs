namespace Etheria.Features.StoryletSystem
{
    public sealed class RemoveEntityAttributeEffect : EntityTargetedEffect
    {
        public RemoveEntityAttributeEffect(RoleId roleId, AttributeId attributeId) : base(roleId)
        {
            AttributeId = attributeId;
        }

        public AttributeId AttributeId { get; }
    }
}
