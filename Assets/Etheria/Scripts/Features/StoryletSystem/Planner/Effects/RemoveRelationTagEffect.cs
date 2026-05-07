namespace Etheria.Features.StoryletSystem
{
    public sealed class RemoveRelationTagEffect : StoryletEffect
    {
        public RemoveRelationTagEffect(RoleId fromRoleId, RoleId toRoleId, TagSet tag)
        {
            FromRoleId = fromRoleId;
            ToRoleId = toRoleId;
            Tag = tag;
        }

        public RoleId FromRoleId { get; }
        public RoleId ToRoleId { get; }
        public TagSet Tag { get; }
    }
}
