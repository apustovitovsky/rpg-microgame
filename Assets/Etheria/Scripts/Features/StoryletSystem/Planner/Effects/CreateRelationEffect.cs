namespace Etheria.Features.StoryletSystem
{
    public sealed class CreateRelationEffect : StoryletEffect
    {
        public CreateRelationEffect(RoleId fromRoleId, RoleId toRoleId, TagSet tags)
        {
            FromRoleId = fromRoleId;
            ToRoleId = toRoleId;
            Tags = tags;
        }

        public RoleId FromRoleId { get; }
        public RoleId ToRoleId { get; }
        public TagSet Tags { get; }
    }
}
