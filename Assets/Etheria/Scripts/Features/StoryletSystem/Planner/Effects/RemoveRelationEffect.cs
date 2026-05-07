namespace Etheria.Features.StoryletSystem
{
    public sealed class RemoveRelationEffect : StoryletEffect
    {
        public RemoveRelationEffect(RoleId fromRoleId, RoleId toRoleId)
        {
            FromRoleId = fromRoleId;
            ToRoleId = toRoleId;
        }

        public RoleId FromRoleId { get; }
        public RoleId ToRoleId { get; }
    }
}
