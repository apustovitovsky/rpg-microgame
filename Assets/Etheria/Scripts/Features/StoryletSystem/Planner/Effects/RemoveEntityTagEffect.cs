namespace Etheria.Features.StoryletSystem
{
    public sealed class RemoveEntityTagEffect : EntityTargetedEffect
    {
        public RemoveEntityTagEffect(RoleId roleId, TagSet tag) : base(roleId)
        {
            Tag = tag;
        }

        public TagSet Tag { get; }
    }
}
